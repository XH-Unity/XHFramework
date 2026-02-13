
Shader "WZY/Sprite/Cloud"
{
    Properties
    {
        [Header(r  ShowArea      g  SelectedArea      b  DissolveValue)][Space]
        _MainTex ("Sprite Texture", 2D) = "white" {}
        [Header(x  Min     y  Max      z  Pow  w alpha  )][Space]
        _MaskRValues("Mask R Values", vector) = (0.1, 1, 1.2, 0.8)//xy 遮罩边缘的范围控制，z Pow与w 投影高度

        [Header(xy  0      z  Distort      w  CloudFirstScale)][Space]
        _ShadeOffset("Shade Offset", vector) = (8, 0.7, 0.75, 2) //xy 点击产生遮罩整体便宜，z云的透明度，w云的比例缩放
        [HideInInspector] [Header(x  Speed     y  Lighting     z  Level     w  TotalAlpha)][Space]
        [HideInInspector] _FlickValues("Flick Values", vector) = (6, 0.6, 0.6, 1) //亮块：x 速度，y 亮度，z 亮与暗的对比度
        [Space(10)]
        _CloudTex("Cloud Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(xy  CloudFirst      zw  CloudSecond)][Space]
        _CloudSpeed("Cloud Speed", vector) = (-0.5, 0, 0.4, 0.25)//xy 第一层云的偏移，zw 第二层云的偏移
        [Space(10)]

        [Space(10)]
        _AlbedoUVRotation("Albedo UV Rotation", Range(-3.14, 3.14)) = 0
        _MaskUVRotation("Mask UV Rotation", Range(-3.14, 3.14)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2
        [Space(10)]
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    CGINCLUDE
    #pragma target 2.0
    #pragma multi_compile_instancing
    #pragma multi_compile _ PIXELSNAP_ON
    #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
    #include "UnitySprites.cginc"


    float4 _MainTex_ST;
    float4 _MaskRValues;
    float4 _ShadeOffset;
    float4 _FlickValues;

    sampler2D _CloudTex;
    float4 _CloudTex_ST;
    float4 _CloudSpeed;

    sampler2D _DissolveTex;
    float4 _DissolveTex_ST;
    float _AlbedoUVRotation;
    float _MaskUVRotation;

    uniform float _MaskGBLerpValue;

    //uv旋转
    float2 RotateUV(float2 uv, float _a)
    {
        float2 curUV = uv - 0.5;

        curUV = float2(curUV.x * cos(_a) - curUV.y * sin(_a), curUV.x * sin(_a) + curUV.y * cos(_a));

        curUV += 0.5;
        return curUV;
    }

    //混合两层云
    float4 BlendTwoCloud(float2 uv)
    {
        float4 c = tex2D(_CloudTex, uv * _ShadeOffset.w + _Time.x * _CloudSpeed.xy);
        float3 c2 = tex2D(_CloudTex, uv + _Time.x * _CloudSpeed.zw);
        c.rgb = (c.rgb + c2) / 2;
        return c;
    }

    //处理mask值
    float ClampAndPowValue(float val, float3 minMaxPow)
    {
        float mValue;
        mValue = saturate((val - minMaxPow.x) / (minMaxPow.y - minMaxPow.x));
        mValue = saturate(pow(mValue, minMaxPow.z));
        return mValue;
    }

    //计算mask   r 显示区域,  g 选中区域,  b 溶解值
    float3 GetMask(float3 c, float2 maskUV, float2 shadowOffset)
    {
        //offset 处理边缘
        float gray = (c.r + c.g + c.b) / 3;
        float2 disOffset = float2(gray - _ShadeOffset.x - shadowOffset.x, gray - _ShadeOffset.y - shadowOffset.y) * _ShadeOffset.z * 0.01; // 遮住偏移：移动与旋转
        //mask
        float4 mask = tex2D(_MainTex, maskUV + disOffset);
        //mask r
        mask.r = ClampAndPowValue(mask.r, _MaskRValues.xyz);

        //mask gb值处理，解决溶解末跳动问题
        float3 maskGBValues = float3(0.3, 1.3, 4);
        mask.g = lerp(mask.g, ClampAndPowValue(mask.g, maskGBValues), _MaskGBLerpValue);
        mask.b = lerp(mask.b, ClampAndPowValue(mask.b, maskGBValues), _MaskGBLerpValue);

        return mask.rgb;
    }
    float3 GetMask(float3 c, float2 maskUV)
    {
        return GetMask(c, maskUV, float2(0, 0));
    }

    //计算溶解
    float DissolveMaskR(float3 mask, float2 dissolveUV)
    {
        float dissolve = 0.9;
        float maskR = lerp(mask.r, lerp(mask.r * smoothstep(0, dissolve, mask.b), mask.r, mask.b), mask.g);
        return maskR;
    }

    struct a2vCloud
    {
        float4 posOS : POSITION;
        float4 vertColor : COLOR;
        float2 uv0 : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2fCloud
    {
        float4 posCS : SV_POSITION;
        float4 color : COLOR;
        float4 maskAndCloudUV : TEXCOORD0;
        float2 dissolveUV : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };


    //cloud顶点计算
    v2fCloud GetCloudVertInputs(float4 posOS, float2 uv0, float4 vertColor)
    {
        v2fCloud input;
        UNITY_SETUP_INSTANCE_ID (IN);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

    #ifdef UNITY_INSTANCING_ENABLED
        posOS.xy *= _Flip;
    #endif

        input.posCS = UnityObjectToClipPos(posOS);
        input.maskAndCloudUV.xy = RotateUV(uv0,_MaskUVRotation);
        input.maskAndCloudUV.zw = RotateUV(TRANSFORM_TEX(uv0, _CloudTex), _AlbedoUVRotation);
        input.dissolveUV = TRANSFORM_TEX(uv0, _DissolveTex);
        input.color = vertColor * _Color * _RendererColor;

    #ifdef PIXELSNAP_ON
        input.posCS = UnityPixelSnap (input.posCS);
    #endif

        return input;
    }


    v2fCloud vertCloud(a2vCloud IN)
    {
        v2fCloud OUT;
        OUT = GetCloudVertInputs(IN.posOS, IN.uv0, IN.vertColor);

        return OUT;
    }



    float4 fragCloud (v2fCloud IN) : SV_Target
    {
        //云贴图采样，两层混合
        float4 c = BlendTwoCloud(IN.maskAndCloudUV.zw);

        //mask  
        float3 mask = GetMask(c, IN.maskAndCloudUV.xy);

        //selected  区域闪烁
        float flicker = lerp(1, sin(_Time.y * _FlickValues.x), _FlickValues.z);
        c.rgb += c.rgb * flicker * mask.g * mask.b * _FlickValues.y;

        //dissolve
        mask.r = DissolveMaskR(mask, IN.dissolveUV);

        //final
        float4 final;
        final.rgb = c.rgb * c.a;
        final.a = c.a;
        final *= IN.color * mask.r;

        final = lerp(0, final, _MaskRValues.w);

        return final;
    }


    ENDCG

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

        Lighting Off
        ZWrite Off
        ZTest[_ZTest]
        Blend[_SrcBlend][_DstBlend]
        Cull[_Cull]



        Pass
        {
            Name "CloudColor"  
            CGPROGRAM
            #pragma vertex vertCloud
            #pragma fragment fragCloud
            ENDCG
        }
    }
}
