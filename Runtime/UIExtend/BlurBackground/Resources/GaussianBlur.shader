Shader "PostEffect/GaussianBlur"
{
    Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlurSpread ("Blur Spread", Range(1.0, 6.0)) = 1.0
	}
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _MainTex_TexelSize;
		    float _BlurSpread;
        CBUFFER_END

         TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);

        struct appdata{
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
        };

        struct v2f {
			float4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
        };

         v2f Vert(appdata v)
        {
			 v2f o;
			 o.pos = TransformObjectToHClip(v.vertex.xyz);
			 o.uv = v.texcoord;
         	return o;
         }
        
		half4 FragBlurH(v2f i) : SV_Target
        {
			float texelSize = _MainTex_TexelSize.x;
        	float2 uv = i.uv;

            half4 c0 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv - float2(texelSize * 4.0, 0.0) * _BlurSpread);
            half4 c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv - float2(texelSize * 3.0, 0.0) * _BlurSpread);
            half4 c2 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv - float2(texelSize * 2.0, 0.0) * _BlurSpread);
            half4 c3 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv - float2(texelSize * 1.0, 0.0) * _BlurSpread);
            half4 c4 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv											  );
            half4 c5 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv + float2(texelSize * 1.0, 0.0) * _BlurSpread);
            half4 c6 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv + float2(texelSize * 2.0, 0.0) * _BlurSpread);
            half4 c7 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv + float2(texelSize * 3.0, 0.0) * _BlurSpread);
            half4 c8 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv + float2(texelSize * 4.0, 0.0) * _BlurSpread);
        	
        	half4 color = c0 * 0.01621622 + c1 * 0.05405405 + c2 * 0.12162162 + c3 * 0.19459459
                        + c4 * 0.22702703
                        + c5 * 0.19459459 + c6 * 0.12162162 + c7 * 0.05405405 + c8 * 0.01621622;

        	return half4(color.xyz, 1);
		}

        half4 FragBlurV(v2f i) : SV_Target
        {
			float texelSize = _MainTex_TexelSize.y;
        	float2 uv = i.uv;

			half4 c0 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv - float2(0.0, texelSize * 4.0) * _BlurSpread);
			half4 c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv - float2(0.0, texelSize * 3.0) * _BlurSpread);
			half4 c2 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv - float2(0.0, texelSize * 2.0) * _BlurSpread);
			half4 c3 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv - float2(0.0, texelSize * 1.0) * _BlurSpread);
			half4 c4 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv											  );
			half4 c5 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv + float2(0.0, texelSize * 1.0) * _BlurSpread);
			half4 c6 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv + float2(0.0, texelSize * 2.0) * _BlurSpread);
			half4 c7 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv + float2(0.0, texelSize * 3.0) * _BlurSpread);
			half4 c8 = SAMPLE_TEXTURE2D(_MainTex, sampler_LinearClamp, uv + float2(0.0, texelSize * 4.0) * _BlurSpread);

        	half4 color = c0 * 0.01621622 + c1 * 0.05405405 + c2 * 0.12162162 + c3 * 0.19459459
                        + c4 * 0.22702703
                        + c5 * 0.19459459 + c6 * 0.12162162 + c7 * 0.05405405 + c8 * 0.01621622;

        	return half4(color.xyz, 1);
		}
        
		    
		ENDHLSL
		
		ZTest Always Cull Off ZWrite Off
		
		Pass {  
			NAME "Blur Horizonal"
			
			HLSLPROGRAM  
			
			#pragma vertex Vert  
			#pragma fragment FragBlurH
			
			ENDHLSL
		}
		
		Pass {
			NAME "Blur Vertical"
			
			HLSLPROGRAM
			  
			#pragma vertex Vert  
			#pragma fragment FragBlurV
			  
			ENDHLSL  
		}
		

	} 
	FallBack "Packages/com.unity.render-pipelines.universal/FallbackError"
}