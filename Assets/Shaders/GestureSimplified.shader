
Shader "Runtime/FullscreenQuad/GestureSimplified"
{
    Properties
    {
        _GestureColor("GestureColor", Vector) = (1, 1, 1, 1)
        _ScreenRatio("ScreenRatio", Float) = 1
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

            in vec4 position;

			vec3 projectPointOntoLine(vec2 point, vec2 lineStart, vec2 lineEnd)
			{
				vec2 line = lineEnd - lineStart;
				float mag = dot(point - lineStart, line) / pow(length(line), 2.0);
				mag = clamp(mag, 0.0, 1.0);
				return vec3(mag * line + lineStart, mag);
			}

			float getRand(float a, float b, float c, float timeScale)
			{
				return fract(sin(dot(vec2(_Time.x * timeScale) ,vec2(a, b))) * c);
			}

            vec4 map(in vec2 pixelCoordinates)
            {
				float col = 0.0;
				float rand1 = getRand(12.9898, 8.233, 43758.5453, 0.0002);
				float rand2 = getRand(8.8921, 33.287, 3558.5734, 0.0003);
                for(int i = 1; i < _Points.length(); i++)
                {
                    if(_Points[i].w == 0 || _Points[i - 1].w == 0)
                    {
                        break;
                    }
					vec3 projection = projectPointOntoLine(pixelCoordinates, _Points[i - 1].xy, _Points[i].xy);
					if(i % 2 == 0)
					{
						rand1, rand2 = rand2, rand1;
					}
					float rand = rand1 + (rand2 - rand1) * projection.z;
					rand = rand * 0.2 + 0.8;
					col += rand * exp(-1000.0 * pow(length(pixelCoordinates - projection.xy), 2.0));
                }
				return vec4(col) * _GestureColor;
            }

            void main() {
				gl_FragColor = map(position.xy);
            }

            #endif

            ENDGLSL
        }
    }
}