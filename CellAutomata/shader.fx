// ���峣����������������Ӧ�ó��򴫵����ݵ���ɫ��
cbuffer ConstantBuffer : register(b0)
{
    matrix WorldViewProjection; // ���ڶ���λ�ñ任������-��ͼ-ͶӰ����
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
    output.Pos = mul(float4(input.Pos, 1.0), WorldViewProjection); // Ӧ�ñ任����
    output.Color = input.Color; // ֱ�Ӵ�����ɫ��������ɫ��
    return output;
}

// ������ɫ��
float4 PS(VertexOutput input) : SV_Target
{
    return input.Color; // ʹ�ö����ṩ����ɫ��Ϊ������ɫ
}

// ������ͨ�����壨�����ڼ�Ӧ�ó�������ɫ����ʹ�ã�
technique10 Render
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_4_0, VS()));
        SetPixelShader(CompileShader(ps_4_0, PS()));
    }
}
