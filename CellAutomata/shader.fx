// 定义常量缓冲区，它将从应用程序传递数据到着色器
cbuffer ConstantBuffer : register(b0)
{
    matrix World;
    matrix View;
    matrix Projection;
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
    
    output.Pos = float4(input.Pos, 1.0f);
    
    // 将输入位置从对象空间转换到裁剪空间
    // output.Pos = mul(float4(input.Pos, 1.0f), World);
    // output.Pos = mul(output.Pos, View);
    // output.Pos = mul(output.Pos, Projection);
    
    output.Color = input.Color; // 直接传递颜色到像素着色器
    return output;
}

// 像素着色器
float4 PS(VertexOutput input) : SV_Target
{
    return input.Color; // 使用顶点提供的颜色作为像素颜色
}