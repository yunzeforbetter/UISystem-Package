Shader "PostEffect/FrostShader"
{
 	Properties
	{
		[PerRendererData] _MainTex ("Fross Texture", 2D) = "white" {}
		_FrostIntensity ("Frost Intensity", Range(0.0, 1.0)) = 0.5
		_Color("Color Tint", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
		LOD 100

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uvfrost : TEXCOORD0;
				float4 uvgrab : TEXCOORD1;  
				float4 vertex : SV_POSITION;
			};

			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			float4 _MainTex_ST;

			float _FrostIntensity;
			float4 _Color;

			TEXTURE2D(_BlurTexture_0); SAMPLER(sampler_BlurTexture_0);
			TEXTURE2D(_BlurTexture_1); SAMPLER(sampler_BlurTexture_1);
			TEXTURE2D(_BlurTexture_2); SAMPLER(sampler_BlurTexture_2);
			TEXTURE2D(_BlurTexture_3); SAMPLER(sampler_BlurTexture_3);
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = TransformObjectToHClip(v.vertex.xyz);
				o.uvfrost = TRANSFORM_TEX(v.uv, _MainTex);
				o.uvgrab = ComputeScreenPos(o.vertex);
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				float surfSmooth = 1- SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uvfrost).r * _FrostIntensity;
				
				surfSmooth = clamp(0, 1, surfSmooth);

				half4 ref00 = SAMPLE_TEXTURE2D(_BlurTexture_0, sampler_BlurTexture_0, i.uvgrab.xy / i.uvgrab.w);
				half4 ref01 = SAMPLE_TEXTURE2D(_BlurTexture_1, sampler_BlurTexture_1, i.uvgrab.xy / i.uvgrab.w);
				half4 ref02 = SAMPLE_TEXTURE2D(_BlurTexture_2, sampler_BlurTexture_2, i.uvgrab.xy / i.uvgrab.w);
				half4 ref03 = SAMPLE_TEXTURE2D(_BlurTexture_3, sampler_BlurTexture_3, i.uvgrab.xy / i.uvgrab.w);
				
				float step00 = smoothstep(0.75, 1.00, surfSmooth);
				float step01 = smoothstep(0.5, 0.75, surfSmooth);
				float step02 = smoothstep(0.05, 0.5, surfSmooth);
				float step03 = smoothstep(0.00, 0.05, surfSmooth);

				half4 refraction = lerp(ref03, lerp( lerp( lerp(ref03, ref02, step02), ref01, step01), ref00, step00), step03);
				refraction.rgb *= _Color.rgb;
				return refraction;
			}
			ENDHLSL
		}
	}
}