Shader "Unlit/LOSShader"
{
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Text Color", Color) = (1,1,1,1)
	}
	 
	Category {
		Tags { "RenderType"="Transparent"}
		Lighting Off
		ZWrite off
	    ZTest always
	    Cull Off
	    Fog { Mode Off }
		BindChannels {
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
	 
		SubShader {
			Blend SrcAlpha OneMinusSrcAlpha
			Pass {
				SetTexture [_MainTex] {
					constantColor [_Color]
					Combine texture * primary DOUBLE
				}
			}
		}
	}
}
