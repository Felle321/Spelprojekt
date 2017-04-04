float4x4 World;
float4x4 View;
float4x4 Projection;
float movX, movY, w, h, k;

sampler s0;

struct VertexShaderInput
{
	float4 Position : POSITION;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);


	return output;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
	float4 color = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float2 newCoords = float2(-1, -1);
	float offsetX, offsetY, w, h;
	float m = 0;

	if (movX < 0)
		w = movX + 1;
	else
		w = 1 - movX;

	if (movY < 0)
		h = movY + 1;
	else
		h = 1 - movY;

	if (movX < 0)
	{
		offsetX = 0;
		
		//Bouth neg
		offsetY = 0;
		m = 1 - k;

		if (movY > 0)
		{
			offsetY = movY;
			m = 1 + k;
		}
	}
	else
	{
		offsetX = movX;
		offsetY = 0;

		if (movY > 0)
		{
			//bouth pos
			offsetY = movY;
		}
	}

	if (coords.x >= offsetX && coords.x <= offsetX + w && coords.y >= offsetY && coords.y <= offsetY + h)
	{
		newCoords.x = (coords.x - offsetX) / w;
		newCoords.y = (coords.y - offsetY) / h;
	}
	else
	{
		if (movX * movY > 0)
		{
			if (coords.y > coords.x * k + m && coords.y < coords.x * k + h)
			{
				return float4(0, 1, 1, 1);
			}
			else if (coords.y < coords.x * k + m && coords.y > (coords.x - w) * k)
			{
				return float4(0, 0, 1, 1);
			}
		}
		else
		{
			if (coords.y > 1 + coords.x * k - m && coords.y < 1 + coords.x * k + h - (1 + k))
			{
				return float4(0, 1, 1, 1);
			}
			else if (coords.y < 1 + coords.x * k - m && coords.y > 1 + (coords.x + w) * k - (1 + k))
			{
				return float4(0, 0, 1, 1);
			}
		}
	}


	if (newCoords.x != -1 && newCoords.y != -1)
		color = tex2D(s0, newCoords);

	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
