// 定义常量缓冲区，它将从应用程序传递数据到着色器
cbuffer ConstantBuffer : register(b0)
{
    matrix WorldViewProjection; // 用于顶点位置变换的世界-视图-投影矩阵
}

// 顶点数据结构
struct VertexInput
{
    float3 Pos : POSITION; // 顶点位置
    float4 Color : COLOR; // 顶点颜色
};

struct VertexOutput
{
    float4 Pos : SV_POSITION; // 顶点位置
    float4 Color : COLOR; // 顶点颜色 
};

// 顶点着色器
VertexOutput VS(VertexInput input)
{
    VertexOutput output;
    output.Pos = mul(float4(input.Pos, 1.0), WorldViewProjection); // 应用变换矩阵
    output.Color = input.Color; // 直接传递颜色到像素着色器
    return output;
}

// 像素着色器
float4 PS(VertexOutput input) : SV_Target
{
    return input.Color; // 使用顶点提供的颜色作为像素颜色
}

// 技术和通道定义（可用于简化应用程序中着色器的使用）
technique10 Render
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_4_0, VS()));
        SetPixelShader(CompileShader(ps_4_0, PS()));
    }
}
