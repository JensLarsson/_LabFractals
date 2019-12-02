// Parameters:
// - Wind Direction;
// - Wind Wave Size;
// - Tree Sway Speed;
// - Tree Sway Displacement;
// - Tree Sway Stutter;
// - Foliage Wiggle Amount;
// - Foliage Wiggle Speed;
// - Branches Up/Down;
// - Red Vertex Influence;
// - Blue Vertex Influence.

Shader "Tree Wind" {

	Properties{
		_MainTex("Main Texture", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)
		_Age("Age", Range (0,1)) = 0

	}

		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 100
			CGPROGRAM

			#pragma target 3.0
			#pragma surface surf Lambert vertex:vert addshadow


			fixed4 _Color;
			sampler2D _MainTex;
			float _Age;


			struct Input {
				float2 uv_MainTex;
			};
			struct appdata {
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float4 uv2 : TEXCOORD1;
			};

			// Vertex Manipulation Function
			void vert(inout appdata_full i) {
				appdata o;
				o.uv = float4(i.texcoord1.xy, 0, 0);
			   float3 worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;
			   i.vertex.x *= _Age;//cos(_Time.z)*o.uv.y*0.1f;
			   i.vertex.y *= _Age;//cos(_Time.z)*o.uv.y*0.05f;
			   // i.normal = o.uv;
			   if (o.uv.y > .95f) {
				   i.vertex.x += cos(_Time.z * 10 + worldPos.x + worldPos.y)*o.uv.y*0.005f;
				   i.vertex.y += cos(_Time.z * 10 + worldPos.x + worldPos.y)*o.uv.y*0.005f;
			   }
			}
			 void surf(Input IN, inout SurfaceOutput o) {
				 fixed4 c = tex2D(_MainTex, IN.uv_MainTex)*_Color;
				 o.Albedo = _Color.rgb  * _Color.a;
				 o.Emission = c.rgb.rgb;
			 }
	 ENDCG
		}

			Fallback "Diffuse"
}