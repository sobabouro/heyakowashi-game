Shader "Custom/LineShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Thickness("Thickness",Range(0.01,1)) = 0.5
		_RightAngle("RightAngleOnly",Range(0,1)) = 1
	}
		SubShader
	{
		Tags {
		"RenderType" = "Opaque" }
		LOD 200


		CGINCLUDE
		
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#include "UnityCG.cginc"

			fixed4 _Color;
			float _Thickness,_RightAngle;


			struct appdata {
				float4 vertex: POSITION;
				float3 normal :NORMAL;
			};

			struct v2g {
				//float4 pos: SV_POSITION;
				float4 vertex:TEXCOORD0;
				float3 normal :NORMAL;
			};

			struct g2f {
				float4 pos :SV_POSITION;
				float3 normal  :NORMAL;
			};

			v2g vert(appdata v) {
				v2g o;
				//o.pos = UnityObjectToClipPos(v.vertex);
				o.vertex = v.vertex;
				o.normal = v.normal;
				return o;
			}


			//[maxvertexcount()]は最大出力数今回は一度のループで15個まで頂点を出力できるようにした
			[maxvertexcount(18)]
            //TriangleStreamは出力されるオブジェクトの形. 他には、PointStream型、LineStream型がある
			void geom(triangle v2g input[3], uint pid : SV_PrimitiveID, inout TriangleStream<g2f> outStream) {
				float3 op[6];
				float3 normals[6];


				for (int i = 0; i < 3; i++) {
					op[i] = input[i].vertex.xyz;
					normals[i] = input[i].normal;
				}
				float weight = 1 / _Thickness;
				for (int i = 0; i < 3; i++) {
					op[i + 3] = op[i] * weight + op[(i + 1) % 3] + op[(i + 2) % 3];
					op[i + 3] /= weight + 2;

					op[i + 3] = op[i] * weight + op[(i + 1) % 3] + op[(i + 2) % 3];
					op[i + 3] /= weight + 2;



					normals[i + 3] = normals[i] * weight + normals[(i + 1) % 3] + normals[(i + 2) % 3];
					normals[i + 3] /= weight + 2;

					normals[i + 3] = normals[i] * weight + normals[(i + 1) % 3] + normals[(i + 2) % 3];
					normals[i + 3] /= weight + 2;

				}
				//float3 normal = normalize(cross(op[1] - op[0], op[2] - op[0]));

				float3 lines[3];

				lines[0] = normalize(op[1] - op[0]);
				lines[1] = normalize(op[2] - op[1]);
				lines[2] = normalize(op[0] - op[2]);

				float rang = _RightAngle;
				float3 newOp[6] = op;
				float3 newNormals[6] = normals;

				for (int i = 0; i < 3; i++) {

					float dotvalue = abs(dot(lines[(i + 1) % 3], lines[(i + 2) % 3]));
					//float dotvalue = abs(dot(lines[0], lines[2]));
					if (rang > dotvalue) {
						rang = dotvalue;

						newOp[i + 3] = op[i] * (weight + 1) + op[(i + 1) % 3];
						newOp[i + 3] /= weight + 2;

						newOp[(i + 1) % 3 + 3] = op[i] + op[(i + 1) % 3] * (weight + 1);
						newOp[(i + 1) % 3 + 3] /= weight + 2;

						newOp[(i + 2) % 3 + 3] = op[(i + 2) % 3 + 3];



						newNormals[i + 3] = normals[i] * (weight + 1) + normals[(i + 1) % 3];
						newNormals[i + 3] /= weight + 2;

						newNormals[(i + 1) % 3 + 3] = normals[i] + normals[(i + 1) % 3] * (weight + 1);
						newNormals[(i + 1) % 3 + 3] /= weight + 2;

						newNormals[(i + 2) % 3 + 3] = normals[(i + 2) % 3 + 3];
					}
				}
				op = newOp;
				normals = newNormals;


				g2f o;
				o.normal = input[0].normal;

				for (int i = 0; i < 3; i++) {

					o.pos = UnityObjectToClipPos(float4(op[i], 1));
					o.normal = normals[i];
					outStream.Append(o);
					o.pos = UnityObjectToClipPos(float4(op[(i + 1) % 3], 1));
					o.normal = normals[(i + 1) % 3];
					outStream.Append(o);
					o.pos = UnityObjectToClipPos(float4(op[i + 3], 1));
					o.normal = normals[i + 3];
					outStream.Append(o);
					outStream.RestartStrip();

					o.pos = UnityObjectToClipPos(float4(op[i + 3], 1));
					outStream.Append(o);
					o.pos = UnityObjectToClipPos(float4(op[(i + 1) % 3], 1));
					o.normal = normals[(i + 1) % 3];
					outStream.Append(o);
					o.pos = UnityObjectToClipPos(float4(op[(i + 1) % 3 + 3], 1));
					o.normal = normals[(i + 1) % 3 + 3];
					outStream.Append(o);
					outStream.RestartStrip();
				}
			}
		

		ENDCG

	   Pass{

			Cull Back
			CGPROGRAM
			fixed4 frag(g2f i) : COLOR
			{
				fixed4 col;
				col = _Color;

				float3 N = normalize(i.normal);
				float3 L = normalize( mul(unity_WorldToObject ,-_WorldSpaceLightPos0));
				

				float light = dot(N, -L)*0.5+0.5;
				col *= (1, 1, 1, 1)*light;


				return col;
			}

			ENDCG
		}


		Pass{

			Cull Front
			CGPROGRAM
			fixed4 frag(g2f i) : COLOR
			{
				fixed4 col;
				col = _Color;

				float3 N = normalize(i.normal);
				float3 L = normalize( mul(unity_WorldToObject ,-_WorldSpaceLightPos0));

				float light = dot(N, L)*0.4+0.4;
				col *= (1, 1, 1, 1)*light;


				return col;
			}

			ENDCG
		}


	}
		FallBack "Diffuse"
}

