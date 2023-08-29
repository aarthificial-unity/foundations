Shader "GUI/BoxSDF"
{
  Properties
  {
    _MainTex ("Sprite Texture", 2D) = "white" {}
    _PaintTex ("Paint Texture", 2D) = "white" {}
    _Radius ("Radius", Float) = 0.3
    _StrokeWidth ("Stroke Width", Float) = 0.1
    _Smoothness ("Smoothness", Float) = 0.01
    _Test ("Test", Vector) = (0, 0, 0, 0)

    [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
    [HideInInspector] _Stencil ("Stencil ID", Float) = 0
    [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
    [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
    [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
    [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    [HideInInspector] [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
  }

  SubShader
  {
    Tags
    {
      "Queue"="Transparent"
      "IgnoreProjector"="True"
      "RenderType"="Transparent"
      "PreviewType"="Plane"
      "CanUseSpriteAtlas"="True"
    }

    Stencil
    {
      Ref [_Stencil]
      Comp [_StencilComp]
      Pass [_StencilOp]
      ReadMask [_StencilReadMask]
      WriteMask [_StencilWriteMask]
    }

    Cull Off
    Lighting Off
    ZWrite Off
    ZTest [unity_GUIZTestMode]
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask [_ColorMask]

    Pass
    {
      Name "Default"
      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma target 2.0

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

      #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
      #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

      struct Attributes
      {
        float4 vertex : POSITION;
        float4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        float4 params : TEXCOORD1;
        float4 params2 : TEXCOORD2;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct Varyings
      {
        float4 vertex : SV_POSITION;
        float4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        float4 worldPosition : TEXCOORD1;
        float4 params : TEXCOORD2;
        float4 params2 : TEXCOORD3;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      sampler2D _PaintTex;
      float4 _PaintTex_ST;

      float _Radius;
      float _StrokeWidth;
      float _Smoothness;
      float4 _ClipRect;
      float4 _Test;

      inline float roundBoxSDF(const float2 position, const float2 size, const float radius)
      {
        float2 q = abs(position) - size + radius;
        return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
      }

      inline float get2DClipping(in float2 position, in float4 clipRect)
      {
        float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
        return inside.x * inside.y;
      }

      Varyings vert(Attributes input)
      {
        UNITY_SETUP_INSTANCE_ID(input);
        Varyings output;
        output.worldPosition = float4(TransformWorldToObject(input.vertex), 0.0);
        output.vertex = TransformObjectToHClip(input.vertex);

        output.texcoord = input.texcoord;
        output.params = input.params;
        output.params2 = input.params2;
        output.color = input.color;
        return output;
      }

      float4 frag(Varyings input) : SV_Target
      {
        half4 color = input.color;
        float2 uv = input.texcoord;
        float2 size = input.params.xy;
        float paddingEdge = -input.params.z * _StrokeWidth * 0.1;
        float strokeEdge = paddingEdge - input.params.w * _StrokeWidth * 0.1;
        float radius = clamp(_Radius, 0, min(size.x, size.y) * 0.5);

        float2 position = (uv - 0.5) * size;
        float distance = roundBoxSDF(position, size * 0.5, radius);

        float2 ddxClipPos = ddx(position);
        float2 ddyClipPos = ddy(position);
        float smoothness = _Smoothness * (abs(ddxClipPos) + abs(ddyClipPos));

        color.a *=
          smoothstep(
            paddingEdge,
            paddingEdge - smoothness,
            distance
          ) *
          smoothstep(
            strokeEdge - smoothness,
            strokeEdge,
            distance
          );

        float paint = tex2D(_PaintTex, (uv * size + input.params2.xy) * _Test.z).b;
        // color.rgb += lerp(input.params2.z, input.params2.w, paint);
        paint -= _Test.x;
        paint *= _Test.y;
        paint *= input.params2.z;
        color.rgb += paint;

        #ifdef UNITY_UI_CLIP_RECT
        color.a *= get2DClipping(input.worldPosition.xy, _ClipRect);
        #endif

        #ifdef UNITY_UI_ALPHACLIP
        clip(color.a - 0.001);
        #endif

        return color;
      }
      ENDHLSL
    }
  }
}
