// ���峣����������������Ӧ�ó��򴫵����ݵ���ɫ��
cbuffer ConstantBuffer : register(b0)
{
    float4 rwhscale; // �������� (pmscale, csizew, csizeh)
    float4 canvasSize; // �������ߴ� (width, height)
}

// �������ݽṹ
struct VertexInput
{
    float3 Pos : POSITION; // ����λ��
    float4 Color : COLOR; // ������ɫ
    int4 InstancePosition : INSTANCE_POSITION; // ʵ��λ�� (x, y, row, col)
};

struct VertexOutput
{
    float4 Pos : SV_POSITION; // ����λ��
    float4 Color : COLOR; // ������ɫ 
};

// ������ɫ��
VertexOutput VS(VertexInput input)
{
    VertexOutput output;

    float pmscale = rwhscale.x;
    float csizew = rwhscale.y; // scaled width
    float csizeh = rwhscale.z; // scaled height

    // ����ʵ����λ��
    int x = input.InstancePosition.x;
    int y = input.InstancePosition.y;
    int col = input.InstancePosition.z;
    int row = input.InstancePosition.w;

    float ndc_x = (float(x) + float(col) * pmscale) / canvasSize.x * 2.0f - 1.0f;
    float ndc_y = 1.0f - (float(y) + float(row + 1) * pmscale) / canvasSize.y * 2.0f;

    // Ӧ��ʵ�������ź�λ�ñ任
    float3 position = input.Pos;
    position.x = position.x * csizew + ndc_x;
    position.y = position.y * csizeh + ndc_y;

    // ��ʵ������Ӧ�õ�����任����ͼ�任��ͶӰ�任������
    output.Pos = float4(position, 1.0f);
    output.Color = input.Color; // ֱ�Ӵ�����ɫ��������ɫ��

    return output;
}

// ������ɫ��
float4 PS(VertexOutput input) : SV_Target
{
    return input.Color; // ʹ�ö����ṩ����ɫ��Ϊ������ɫ
}