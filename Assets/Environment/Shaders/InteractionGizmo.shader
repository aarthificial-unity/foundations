Shader "GUI/InteractionGizmo"
{
  Properties
  {
    _Color ("Color", Color) = (1, 1, 1, 1)
    _LTColor ("LT Color", Color) = (0, 0, 1, 1)
    _MidColor ("Mid Color", Color) = (1, 0, 1, 1)
    _RTColor ("RT Color", Color) = (1, 0, 0, 1)
    _Smoothness ("Smoothness", Float) = 1
    _IconScales ("Icon Scales", Vector) = (2.23, 2.23, 2.23, 1)
    _Icon ("Icon", Vector) = (0, 0, 0, 0)

    [HideInInspector] _DialogueRect ("Dialogue Rect", Vector) = (0, 0, 0, 0)
    [HideInInspector] _SkipRect ("Skip Rect", Vector) = (0, 0, 0, 0)
    [HideInInspector] _PlayRect ("Play Rect", Vector) = (0, 0, 0, 0)
    [HideInInspector] _CancelRect ("Cancel Rect", Vector) = (0, 0, 0, 0)
    [HideInInspector] _AtlasTex ("Atlas Texture", 2D) = "white" {}

    [HideInInspector] _Direction ("Direction", Vector) = (1, 0, 0, 0)
    [HideInInspector] _State ("State", Vector) = (0, 0, 0, 0)
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

    Cull Off
    Lighting Off
    ZWrite Off
    ZTest Always
    Blend One OneMinusSrcAlpha

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
        float4 positionOS : POSITION;
        float4 color : COLOR;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct Varyings
      {
        float4 positionCS : SV_POSITION;
        float4 color : COLOR;
        float2 uv : TEXCOORD0;
        float4 positionOS : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float4 _LTColor;
        float4 _RTColor;
        float4 _MidColor;
        float _Smoothness;
        float4 _IconScales;
        float4 _DialogueRect;
        float4 _SkipRect;
        float4 _PlayRect;
        float4 _CancelRect;
        float4 _Icon;
        sampler2D _AtlasTex;
        float4 _AtlasTex_TexelSize;
      CBUFFER_END

      float4 _State;
      float4 _Direction;

      inline float circleSDF(const float2 position, const float radius)
      {
        return length(position) - radius;
      }

      inline float fromTo(const float smoothness, const float distance, const float from, const float to)
      {
        return
          smoothstep(from + smoothness, from - smoothness, distance) *
          smoothstep(to - smoothness, to + smoothness, distance);
      }

      inline float get2DClipping(in float2 position, in float4 clipRect)
      {
        float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
        return inside.x * inside.y;
      }

      float cubicEaseInOut(float t)
      {
        t = saturate(t);
        float t2 = t * t;
        return t2 * (3.0 - 2.0 * t);
      }

      float iconSDF(const float2 uv, const float4 rect, const float scale)
      {
        const float ratio = rect.z / rect.w;
        const float2 iconUV = (uv - 0.5) * scale * float2(1 / ratio, 1) + 0.5;
        float distance = tex2D(_AtlasTex, iconUV * rect.zw + rect.xy).a;
        distance *= get2DClipping(iconUV, float4(0, 0, 1, 1));
        distance = (0.5 - distance) / scale / 3.0;
        return distance;
      }

      Varyings vert(Attributes input)
      {
        UNITY_SETUP_INSTANCE_ID(input);
        Varyings output;

        const float scale = 0.5 + lerp(1, 0.58333333333333, _State.x);
        const float4 positionOS = input.positionOS * scale;
        output.positionOS = positionOS;
        output.positionCS = TransformObjectToHClip(positionOS);

        output.uv = (input.uv - 0.5) * scale + 0.5;
        output.color = input.color;
        return output;
      }

      float4 frag(Varyings input) : SV_Target
      {
        float expansion = _State.x;
        float opacity = _State.y;
        float playerType = _State.z;
        float playerPresence = _State.w;

        float2 ddxClipPos = ddx(input.positionOS.xy);
        float2 ddyClipPos = ddy(input.positionOS.xy);
        float smoothness = _Smoothness * (abs(ddxClipPos) + abs(ddyClipPos));

        float circleDistance = circleSDF(input.uv - 0.5, 0.5);
        float strokeOffset = lerp(0.0, (1 - 0.58333333333333) * -0.5, expansion);

        float2 uvCenter = normalize(input.uv - 0.5);
        uvCenter += _Direction.xy * (playerType * 4 - 2);
        float colorDot = saturate((dot(_Direction.xy, uvCenter) * 4 + 1) * 0.5);
        colorDot = saturate(cubicEaseInOut(colorDot));

        half4 playerColor = lerp(_LTColor, _MidColor, saturate(colorDot * 2));
        playerColor = lerp(playerColor, _RTColor, saturate(colorDot * 2 - 1));
        playerColor = lerp(_Color, playerColor, playerPresence);
        half4 color = fromTo(smoothness, circleDistance, strokeOffset, strokeOffset - 7 / 96.0) * playerColor;

        float4 iconScales = _IconScales / lerp(1, 0.5833333, saturate(expansion));
        float4 icons = float4(
          iconSDF(input.uv, _DialogueRect, iconScales.x),
          iconSDF(input.uv, _PlayRect, iconScales.y),
          iconSDF(input.uv, _SkipRect, iconScales.z),
          iconSDF(input.uv, _CancelRect, iconScales.w)
        );

        float iconDistance = dot(icons, _Icon) / dot(_Icon, float4(1, 1, 1, 1));
        float innerCircle = circleDistance - (1 - 0.58333333333333) * -0.5 + (16 / 96.0);
        iconDistance = lerp(iconDistance, innerCircle, saturate(expansion * 1.25));
        color += _Color * smoothstep(smoothness, -smoothness, iconDistance);
        color *= opacity;

        return color;
      }
      ENDHLSL
    }
  }
}
