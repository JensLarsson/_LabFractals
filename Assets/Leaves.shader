Shader "Leaves" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Colour", Color) = (0,0,0,1)
		_Noise("Noise", 2D) = "white" {}
	}
		SubShader{
		  Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		  CGPROGRAM
		  #pragma surface surf Lambert vertex:vert alpha

		struct Input {
			  float2 uv_MainTex;
			  float2 leafuv;
		  };

		struct appdata {
			float4 vertex : POSITION;
			float4 uv : TEXCOORD0;	//Texture
			float4 uv2 : TEXCOORD1;	//Physics
			float4 uv3 : TEXCOORD2;
		};


		sampler2D _MainTex;
		sampler2D _Noise;
		float4 _Color;


		  void vert(inout appdata_full v, out Input l) {
			  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			  UNITY_INITIALIZE_OUTPUT(Input, l);
			  l.leafuv = v.texcoord3;
			  appdata o;
			  o.uv2 = float4(v.texcoord1.xy, 0, 0);
			  float f = tex2Dlod(_Noise, float4(worldPos.xy / 100, 0,0)).r * 100;

			  v.vertex.x += cos(f + _Time.z*1.2)*o.uv2.y*0.1f;
			  v.vertex.y += sin(f + _Time.z*1.9)*o.uv2.y*0.05f;

			  //if (o.uv2.y > .95f) {
				//v.vertex.x += cos(tex2Dlod(_Noise, float4((f+_Time.y) % 1, (f+_Time.y) % 1, 0, 0)))*.1;
				//v.vertex.y += cos(tex2Dlod(_Noise, float4((f+_Time.y) % 1, (f+_Time.y) % 1, 0, 0)))*.1;
			  //}
		  }



		  void surf(Input IN, inout SurfaceOutput o) {

			  o.Alpha = 0;
			  if (IN.leafuv.x > 0) {
				  fixed4 c = tex2D(_MainTex, IN.leafuv);
				  o.Albedo = (c.rgb  * c.a);
				  o.Emission = c.rgb.rgb;
				  o.Alpha = c.a*_Color.a;
			  }
		  }
		  ENDCG
	}
		Fallback "Diffuse"
}