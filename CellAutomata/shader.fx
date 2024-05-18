// 定义常量缓冲区，它将从应用程序传递数据到着色器
cbuffer ConstantBuffer : register(b0)
{
    float4 rwhscale; // 缩放因子 (pmscale, csizew, csizeh)
    float4 canvasSize; // 增画布尺寸 (width, height)
}

// 顶点数据结构
struct VertexInput
{
    float3 Pos : POSITION; // 顶点位置
    float4 Color : COLOR; // 顶点颜色
    int4 InstancePosition : INSTANCE_POSITION; // 实例位置 (x, y, row, col)
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

    float pmscale = rwhscale.x;
    float csizew = rwhscale.y; // scaled width
    float csizeh = rwhscale.z; // scaled height

    // 计算实例的位置
    int x = input.InstancePosition.x;
    int y = input.InstancePosition.y;
    int col = input.InstancePosition.z;
    int row = input.InstancePosition.w;

    float ndc_x = (float(x) + float(col) * pmscale) / canvasSize.x * 2.0f - 1.0f;
    float ndc_y = 1.0f - (float(y) + float(row + 1) * pmscale) / canvasSize.y * 2.0f;

    // 应用实例的缩放和位置变换
    float3 position = input.Pos;
    position.x = position.x * csizew + ndc_x;
    position.y = position.y * csizeh + ndc_y;

    // 将实例坐标应用到世界变换、视图变换和投影变换矩阵上
    output.Pos = float4(position, 1.0f);
    output.Color = input.Color; // 直接传递颜色到像素着色器

    return output;
}

// 像素着色器
float4 PS(VertexOutput input) : SV_Target
{
    return input.Color; // 使用顶点提供的颜色作为像素颜色
}