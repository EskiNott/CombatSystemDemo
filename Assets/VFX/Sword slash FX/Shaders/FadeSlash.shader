Shader "ERB/Particles/FadeSlash"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_EmissionRGB("Emission/R/G/B", Vector) = (1,1,0.5,0.5)
		_Startopacity("Start opacity", Float) = 40
		[Toggle]_Sideopacity("Side opacity", Float) = 0
		_Sideopacitypower("Side opacity power", Float) = 40
		_Finalopacity("Final opacity", Range( 0 , 1)) = 1
		[Toggle]_Distortion("Distortion", Float) = 0
		_Distortionpower("Distortion power", Range( 0 , 2)) = 1
		_Noise("Noise", 2D) = "white" {}
		_LenghtSet1ifyouuseinPS("Lenght(Set 1 if you use in PS)", Range( 0 , 1)) = 1
		_PathSet0ifyouuseinPS("Path(Set 0 if you use in PS)", Range( 0 , 1)) = 0
		_NoisespeedXY("Noise speed XY", Vector) = (0,0,0,0)
		[Enum(Cull Off,0, Cull Front,1, Cull Back,2)] _CullMode("Culling", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _tex3coord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull[_CullMode]
		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
		
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float2 uv_texcoord;
			float3 uv2_tex3coord2;
			float4 vertexColor : COLOR;
		};

		uniform float _Distortion;
		uniform sampler2D _GrabTexture;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Distortionpower;
		uniform sampler2D _Noise;
		uniform float4 _NoisespeedXY;
		uniform float4 _Noise_ST;
		uniform float _PathSet0ifyouuseinPS;
		uniform float _LenghtSet1ifyouuseinPS;
		uniform float4 _EmissionRGB;
		uniform float4 _Color;
		uniform float _Startopacity;
		uniform float _Sideopacity;
		uniform float _Sideopacitypower;
		uniform float _Finalopacity;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv0_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float U51 = uv0_MainTex.x;
			float2 appendResult79 = (float2(_NoisespeedXY.x , _NoisespeedXY.y));
			float2 uv0_Noise = i.uv_texcoord * _Noise_ST.xy + _Noise_ST.zw;
			float2 panner81 = ( 1.0 * _Time.y * appendResult79 + uv0_Noise);
			float NoiseR70 = tex2D( _Noise, panner81 ).r;
			float temp_output_47_0 = ( _Distortionpower * NoiseR70 );
			float2 appendResult49 = (float2(U51 , ( temp_output_47_0 * temp_output_47_0 )));
			float4 screenColor41 = tex2D( _GrabTexture, appendResult49 );
			float3 clampResult42 = clamp( (screenColor41).rgb , float3( 0,0,0 ) , float3( 1,1,1 ) );
			float temp_output_7_0 = (2.5 + (( _PathSet0ifyouuseinPS + i.uv2_tex3coord2.x ) - 0.0) * (1.0 - 2.5) / (1.0 - 0.0));
			float clampResult76 = clamp( i.uv2_tex3coord2.y , 0.0 , 1.0 );
			float temp_output_10_0 = (1.0 + (( _LenghtSet1ifyouuseinPS * clampResult76 ) - 0.0) * (0.0 - 1.0) / (1.0 - 0.0));
			float clampResult16 = clamp( ( ( ( temp_output_7_0 * temp_output_7_0 * temp_output_7_0 * temp_output_7_0 * temp_output_7_0 ) * U51 ) - temp_output_10_0 ) , 0.0 , 1.0 );
			float V66 = uv0_MainTex.y;
			float2 appendResult18 = (float2(( clampResult16 * ( 1.0 / (1.0 + (temp_output_10_0 - 0.0) * (0.001 - 1.0) / (1.0 - 0.0)) ) ) , V66));
			float2 clampResult19 = clamp( appendResult18 , float2( 0,0 ) , float2( 1,1 ) );
			float4 tex2DNode21 = tex2D( _MainTex, clampResult19 );
			o.Emission = ( lerp(float3( 0,0,0 ),clampResult42,_Distortion) + (( ( ( tex2DNode21.r * _EmissionRGB.y ) + ( tex2DNode21.g * _EmissionRGB.z ) + ( tex2DNode21.b * _EmissionRGB.w ) ) * i.vertexColor * _Color * _EmissionRGB.x )).rgb );
			float temp_output_54_0 = (clampResult19).x;
			float clampResult59 = clamp( ( (1.0 + (temp_output_54_0 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) * _Startopacity ) , 0.0 , 1.0 );
			float clampResult60 = clamp( ( _Startopacity * temp_output_54_0 ) , 0.0 , 1.0 );
			float clampResult62 = clamp( ( (1.0 + (V66 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) * _Sideopacitypower ) , 0.0 , 1.0 );
			float clampResult28 = clamp( ( tex2DNode21.a * i.vertexColor.a * _Color.a * ( clampResult59 * clampResult60 * lerp(1.0,clampResult62,_Sideopacity) ) * _Finalopacity ) , 0.0 , 1.0 );
			float W86 = i.uv2_tex3coord2.z;
			o.Alpha = ( clampResult28 * step( NoiseR70 , W86 ) );
		}

		ENDCG
	}
}
/*ASEBEGIN
Version=16700
649;92;984;655;1419.179;613.1012;2.162071;True;False
Node;AmplifyShaderEditor.CommentaryNode;53;-4594.897,-31.66151;Float;False;2282.792;622.2435;Slash UV;21;2;3;4;7;8;5;12;9;10;13;14;16;17;18;19;51;66;75;76;85;86;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-4534.782,36.47683;Float;False;Property;_PathSet0ifyouuseinPS;Path(Set 0 if you use in PS);11;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-4580.993,291.1873;Float;False;1;-1;3;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;4;-4086.843,39.92331;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;76;-4122.727,246.3413;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-4544.898,157.054;Float;False;Property;_LenghtSet1ifyouuseinPS;Lenght(Set 1 if you use in PS);10;0;Create;True;0;0;False;0;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;85;-4131.484,382.9394;Float;False;0;21;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;7;-3958.908,30.9539;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;2.5;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-3753.345,26.11986;Float;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-3764.578,197.5099;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;51;-3845.306,316.1281;Float;False;U;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-3591.818,25.15206;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;82;-3348.948,-521.6219;Float;False;668.4646;345.2865;Noise moving;4;80;79;81;84;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;10;-3596.354,195.7614;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;13;-3363.758,24.30754;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;80;-3286.561,-359.0679;Float;False;Property;_NoisespeedXY;Noise speed XY;13;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;14;-3361.15,183.8625;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0.001;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;16;-3117.833,19.53228;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;75;-3112.053,162.2986;Float;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;84;-3298.185,-476.5242;Float;False;0;44;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;79;-3072.796,-358.1245;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;71;-2605.254,-573.6427;Float;False;1807.412;418.4509;dISTORTION;11;46;47;48;50;49;41;43;42;20;70;44;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PannerNode;81;-2887.483,-469.6263;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;66;-3840.609,391.7113;Float;False;V;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-2923.243,18.3385;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;83;-2398.328,730.9493;Float;False;1396.689;645.0528;Side opacity;14;67;65;54;64;56;55;63;57;58;62;61;60;59;25;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;18;-2700.386,193.8979;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;44;-2555.254,-385.1909;Float;True;Property;_Noise;Noise;9;0;Create;True;0;0;False;0;7f18ea778cc5a5841baf3dbfb2d768ff;7f18ea778cc5a5841baf3dbfb2d768ff;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;46;-2329.526,-523.6426;Float;False;Property;_Distortionpower;Distortion power;8;0;Create;True;0;0;False;0;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;19;-2507.051,193.3815;Float;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;67;-2345.139,1079.222;Float;False;66;V;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;70;-2265.543,-354.6309;Float;False;NoiseR;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-2034.53,-366.6419;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;54;-2149.501,982.7732;Float;False;True;False;True;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-2207.309,1261.002;Float;False;Property;_Sideopacitypower;Side opacity power;5;0;Create;True;0;0;False;0;40;40;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;65;-2168.895,1085.672;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-1897.029,942.9186;Float;False;Property;_Startopacity;Start opacity;3;0;Create;True;0;0;False;0;40;40;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;55;-1901.338,780.9493;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-1926.263,-459.459;Float;False;51;U;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-1927.908,1114.902;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-1876.53,-373.6419;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;49;-1725.53,-426.6422;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;24;-1027.797,526.1595;Float;False;Property;_EmissionRGB;Emission/R/G/B;2;0;Create;True;0;0;False;0;1,1,0.5,0.5;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;21;-1055.309,-73.36416;Float;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;0d8d0422437fa7246a69d47348c41602;0d8d0422437fa7246a69d47348c41602;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-1678.167,967.9112;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;62;-1733.804,1077.494;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-1668.055,800.1371;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;41;-1586.903,-433.8066;Float;False;Global;_GrabScreen0;Grab Screen 0;8;0;Create;True;0;0;False;0;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-650.4408,74.78885;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-647.9492,-19.89673;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-645.4576,-109.5992;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;59;-1519.85,799.2576;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;60;-1523.831,946.5353;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;61;-1582.211,1068.601;Float;False;Property;_Sideopacity;Side opacity;4;0;Create;True;0;0;False;0;0;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;23;-1224.055,331.3597;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;37;-847.563,492.4088;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;33;-492.3318,-39.06543;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;86;-4275.782,502.4479;Float;False;W;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;43;-1416.722,-431.7076;Float;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-1170.639,820.9329;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-906.1818,853.6483;Float;False;Property;_Finalopacity;Final opacity;6;0;Create;True;0;0;False;0;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;92;-902.5821,1036.698;Float;False;570.9741;327.8997;CutOff;4;69;72;91;87;;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;22;-1145.052,142.3896;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-343.5527,154.728;Float;False;4;4;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;72;-673.3766,1086.698;Float;False;70;NoiseR;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;42;-1196.407,-430.2734;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;69;-852.5821,1159.15;Float;False;86;W;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-592.9656,567.994;Float;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;20;-1025.846,-427.0848;Float;False;Property;_Distortion;Distortion;7;0;Create;True;0;0;False;0;0;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StepOpNode;95;-297.4688,1245.343;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;28;-405.2354,569.4601;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;35;-183.4849,147.1582;Float;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;38.26844,85.49657;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;91;-452.0996,1072.662;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;87;-676.9204,1162.598;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.5;False;4;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;94;-67.26353,528.1059;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;193.5608,74.98804;Float;False;True;2;Float;;0;0;Unlit;ERB/Particles/FadeSlash;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;TransparentCutout;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;12;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;2;0
WireConnection;4;1;5;1
WireConnection;76;0;5;2
WireConnection;7;0;4;0
WireConnection;8;0;7;0
WireConnection;8;1;7;0
WireConnection;8;2;7;0
WireConnection;8;3;7;0
WireConnection;8;4;7;0
WireConnection;9;0;3;0
WireConnection;9;1;76;0
WireConnection;51;0;85;1
WireConnection;12;0;8;0
WireConnection;12;1;51;0
WireConnection;10;0;9;0
WireConnection;13;0;12;0
WireConnection;13;1;10;0
WireConnection;14;0;10;0
WireConnection;16;0;13;0
WireConnection;75;1;14;0
WireConnection;79;0;80;1
WireConnection;79;1;80;2
WireConnection;81;0;84;0
WireConnection;81;2;79;0
WireConnection;66;0;85;2
WireConnection;17;0;16;0
WireConnection;17;1;75;0
WireConnection;18;0;17;0
WireConnection;18;1;66;0
WireConnection;44;1;81;0
WireConnection;19;0;18;0
WireConnection;70;0;44;1
WireConnection;47;0;46;0
WireConnection;47;1;70;0
WireConnection;54;0;19;0
WireConnection;65;0;67;0
WireConnection;55;0;54;0
WireConnection;63;0;65;0
WireConnection;63;1;64;0
WireConnection;48;0;47;0
WireConnection;48;1;47;0
WireConnection;49;0;50;0
WireConnection;49;1;48;0
WireConnection;21;1;19;0
WireConnection;58;0;56;0
WireConnection;58;1;54;0
WireConnection;62;0;63;0
WireConnection;57;0;55;0
WireConnection;57;1;56;0
WireConnection;41;0;49;0
WireConnection;31;0;21;3
WireConnection;31;1;24;4
WireConnection;30;0;21;2
WireConnection;30;1;24;3
WireConnection;29;0;21;1
WireConnection;29;1;24;2
WireConnection;59;0;57;0
WireConnection;60;0;58;0
WireConnection;61;1;62;0
WireConnection;37;0;23;4
WireConnection;33;0;29;0
WireConnection;33;1;30;0
WireConnection;33;2;31;0
WireConnection;86;0;5;3
WireConnection;43;0;41;0
WireConnection;25;0;59;0
WireConnection;25;1;60;0
WireConnection;25;2;61;0
WireConnection;34;0;33;0
WireConnection;34;1;22;0
WireConnection;34;2;23;0
WireConnection;34;3;24;1
WireConnection;42;0;43;0
WireConnection;27;0;21;4
WireConnection;27;1;22;4
WireConnection;27;2;37;0
WireConnection;27;3;25;0
WireConnection;27;4;26;0
WireConnection;20;1;42;0
WireConnection;95;0;72;0
WireConnection;95;1;69;0
WireConnection;28;0;27;0
WireConnection;35;0;34;0
WireConnection;39;0;20;0
WireConnection;39;1;35;0
WireConnection;91;0;72;0
WireConnection;91;1;87;0
WireConnection;87;0;69;0
WireConnection;94;0;28;0
WireConnection;94;1;95;0
WireConnection;0;2;39;0
WireConnection;0;9;94;0
ASEEND*/
//CHKSM=45597E2F7753F07EADA7E55E59E7C01760AF6401