
Shader "Runtime/FullscreenQuad/Gesture"
{
    Properties
    {
        _ScreenRatio("ScreenRatio", Float) = 1.0

        _CameraPosition("CameraPosition", Vector) = (3, 3, 3, 0)
        _CameraTarget("CameraTarget", Vector) = (0, 0, 0, 0)
        _CameraClippingNear("CameraClippingNear", Float) = 0.0
        _CameraClippingFar("CameraClippingFar", Float) = 1000.0

        _RayTracingStep("RayTracingStep", Float) = 0.0001

        _NormalsStep("NormalsStep", Float) = 0.0001

        _GestureColor("GestureColor", Vector) = (1, 1, 1, 0)
    }

    SubShader
    {
		Tags { "Queue" = "Transparent" }
        Cull Off ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            GLSLPROGRAM
            #include "UnityCG.glslinc"

            uniform highp float _ScreenRatio;

            uniform highp vec3 _CameraPosition;
            uniform highp vec3 _CameraTarget;
            uniform highp float _CameraClippingNear;
            uniform highp float _CameraClippingFar;

            uniform highp float _RayTracingStep;

            uniform highp float _NormalsStep;
			
            uniform highp vec4 _GestureColor;

            uniform highp vec4 _Points[10];
            #ifdef VERTEX
            out vec4 position;
            void main() {
                position = gl_Vertex + vec4(0.5, 0.5, 0.5, 0.5);
                float x = gl_Vertex.x > 0 ? 1 : -1;
                float y = gl_Vertex.y > 0 ? 1 : -1;
                float z = gl_Vertex.z > 0 ? 1 : -1;
                float w = gl_Vertex.w > 0 ? 1 : -1;
                gl_Position = vec4(x, y, z, w);
            }
            #endif

            #ifdef FRAGMENT
            #define VEC3_UP vec3(0.0, 1.0, 0.0)
            in vec4 position;

            struct PointMeshProperties
            {
                float deep;
                vec3 normal;
            };

            struct PointMaterialProperties
            {
                vec4 diffuseColor;
                vec4 emissionColor;
                float fresnelStrength;
            };

            struct PointProperties
            {
                PointMeshProperties meshProperties;
                PointMaterialProperties materialProperties;
            };

            PointProperties sdPulsingLine(vec3 point1, vec3 point2, vec3 cameraPosition, vec3 rayDirection, float sharpness)
            {
                vec3 ba = point2 - point1;
                vec3 oa = cameraPosition - point1;
                
                float oad = dot(oa, rayDirection);
                float dba = dot(rayDirection, ba);
                float baba = dot(ba, ba);
                float oaba = dot(oa, ba);
                
                vec2 th = vec2(-oad * baba + dba * oaba, oaba - oad * dba) / (baba - dba * dba);
                
                th.x = max(th.x, 0.0);
                th.y = clamp(th.y, 0.0, 1.0);
                
                vec3 p = point1 + ba * th.y;
                vec3 q = cameraPosition + rayDirection * th.x;

                float rand1 = fract(sin(dot(vec2(_Time) ,vec2(12.9898, 78.233))) * 43758.5453);
                float rand2 = fract(sin(dot(vec2(_Time) ,vec2(89.8921, 33.287))) * 35458.5734);
                float rand = rand1 + (rand2 - rand1) * th.y;
                rand = rand * 0.3 + 0.7;

                PointMeshProperties mesh = PointMeshProperties(
                    exp(-sharpness * length(p - q) * length(p - q)),
                    normalize(cameraPosition));
                mesh.deep = mesh.deep > 0.0 ? th.x * 0.5 : _CameraClippingFar;
                float col = rand * 0.5 * exp(-sharpness / 30.0 * length(p - q) * length(p - q));
                PointMaterialProperties mat = PointMaterialProperties(
                    vec4(col),
                    vec4(col),
                    0.0);

                return PointProperties(
                    mesh,
                    mat);
            }

//------------------------------------------------------------------

            PointProperties opAdd(PointProperties d1, PointProperties d2)
            {
                d1.materialProperties.diffuseColor += d2.materialProperties.diffuseColor;
                d2.materialProperties.diffuseColor = d1.materialProperties.diffuseColor;
                return (d1.meshProperties.deep <= d2.meshProperties.deep) ? d1 : d2;
            }

//------------------------------------------------------------------

            PointProperties map(in vec3 pos, in vec3 rayDirection, in bool isColorCalculated)
            {
                PointProperties res = PointProperties(
                    PointMeshProperties(1.0, VEC3_UP),
                    PointMaterialProperties(vec4(0.0), vec4(0.0), 0.0));

                for(int i = 1; i < _Points.length(); i++)
                {
                    if(_Points[i].w == 0 || _Points[i - 1].w == 0)
                    {
                        break;
                    }
                    PointProperties pointProperties = sdPulsingLine(
                        _Points[i - 1].xyz * vec3(_ScreenRatio, 1.0, 1.0),
                        _Points[i].xyz * vec3(_ScreenRatio, 1.0, 1.0),
                        pos,
                        rayDirection,
                        100.0);
					pointProperties.materialProperties.diffuseColor *= _GestureColor;
                    res = opAdd(res, pointProperties);
                }

                return res;
            }

            PointProperties castRay(in vec3 rayDirection)
            {
                PointProperties res = PointProperties(
                    PointMeshProperties(0.0, VEC3_UP),
                    PointMaterialProperties(vec4(0.0), vec4(0.0), 0.0));
                float deepSum = _CameraClippingNear;
                for(int i = 0; true; i++)
                {
                    res = map(_CameraPosition + rayDirection * deepSum, rayDirection, false);
                    if(res.meshProperties.deep < _RayTracingStep || deepSum > _CameraClippingFar)
                    {
                        break;
                    }
                    deepSum += res.meshProperties.deep;
                }
                res = map(_CameraPosition + rayDirection * deepSum, rayDirection, true);
                res.meshProperties.deep = deepSum;
                if(deepSum > _CameraClippingFar)
                {
                    res.materialProperties.diffuseColor = -1.0 * vec4(1.0, 1.0, 1.0, 0.0);
                }
                return res;
            }

            vec4 render(in vec3 rayDirection)
            {
                PointProperties pointProperties = castRay(rayDirection);
                return vec4(clamp(pointProperties.materialProperties.diffuseColor, 0.0, 1.0));
            }

            mat3 setCamera(in vec3 _CameraPosition, in vec3 _CameraTarget, float cameraRotation)
            {
                vec3 cw = normalize(_CameraTarget - _CameraPosition);
                vec3 cp = vec3(sin(cameraRotation), cos(cameraRotation), 0.0);
                vec3 cu = normalize(cross(cw, cp));
                vec3 cv = normalize(cross(cu, cw));
                return mat3(cu, cv, cw);
            }

            void main() {
                vec2 pixelCoordinates = (2.0 * position.xy - vec2(1.0, 1.0));
                pixelCoordinates.x *= _ScreenRatio;
                float cameraRotation = 0.0;
                mat3 cameraMatrix = setCamera(_CameraPosition, _CameraTarget, cameraRotation);
                vec3 rayDirection = cameraMatrix * normalize(vec3(pixelCoordinates.xy, 2.0));
                gl_FragColor = render(rayDirection);
            }

            #endif

            ENDGLSL
        }
    }
}