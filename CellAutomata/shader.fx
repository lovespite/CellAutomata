// ���峣����������������Ӧ�ó��򴫵����ݵ���ɫ��
cbuffer ConstantBuffer : register(b0)
{
    matrix World;
    matrix View;
    matrix Projection;
}

// �������ݽṹ
struct VertexInput
{
    float3 Pos : POSITION; // ����λ��
    float4 Color : COLOR; // ������ɫ
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
    
    output.Pos = float4(input.Pos, 1.0f);
    
    // ������λ�ôӶ���ռ�ת�����ü��ռ�
    // output.Pos = mul(float4(input.Pos, 1.0f), World);
    // output.Pos = mul(output.Pos, View);
    // output.Pos = mul(output.Pos, Projection);
    
    output.Color = input.Color; // ֱ�Ӵ�����ɫ��������ɫ��
    return output;
}

// ������ɫ��
float4 PS(VertexOutput input) : SV_Target
{
    return input.Color; // ʹ�ö����ṩ����ɫ��Ϊ������ɫ
}