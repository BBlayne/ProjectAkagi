using UnityEngine;
using System.Collections.Generic;

using UnityStandardAssets.ImageEffects;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class HighlightsPostEffect : MonoBehaviour 
{
	#region enums
	public enum HighlightType
	{
		Glow = 0,
		Solid = 1
	}
	public enum SortingType
	{
		Overlay = 3,
		DepthFilter = 4
	}
	public enum FillType
	{
		Fill,
		Outline
	}
	public enum RTResolution
	{
		Quarter = 4,
		Half = 2,
		Full = 1
	}
	#endregion

	#region public vars

	public HighlightType m_selectionType = HighlightType.Glow;
	public SortingType m_sortingType = SortingType.DepthFilter;	
	public FillType m_fillType = FillType.Outline;
	public RTResolution m_resolution = RTResolution.Full;

	public string m_occludeesTag = "Occludee";
	public string m_occludersTag = "Occluder";
	public Color m_highlightColor = new Color(1f, 0f, 0f, 0.65f);

	public Shader m_highlightShader;

	#endregion

	#region private field

	private BlurOptimized m_blur;

	private Renderer[] highlightObjects;
	private Renderer[] m_occluders = null;
	
	private Material m_highlightMaterial;
	
	private CommandBuffer m_renderBuffer;

	private int m_RTWidth = 512;
	private int m_RTHeight = 512;

	#endregion

	private void Awake()
	{
		CreateBuffers();
		CreateMaterials();
		SetOccluderObjects();

        //m_blur = gameObject.AddComponent<BlurOptimized>();
        m_blur = gameObject.GetComponent<BlurOptimized>();
		m_blur.enabled = false;

		GameObject[] occludees = GameObject.FindGameObjectsWithTag(m_occludeesTag);
		highlightObjects = new Renderer[occludees.Length];


		for( int i = 0; i < occludees.Length; i++ )
			highlightObjects[i] = occludees[i].GetComponent<Renderer>();

		m_RTWidth = (int) (Screen.width / (float) m_resolution);
		m_RTHeight = (int) (Screen.height / (float) m_resolution);
	}

	private void CreateBuffers()
	{
		m_renderBuffer = new CommandBuffer();
	}

	private void ClearCommandBuffers()
	{
		m_renderBuffer.Clear();
	}
	
	private void CreateMaterials()
	{
		m_highlightMaterial = new Material( m_highlightShader );
	}

	private void SetOccluderObjects()
	{
		if( string.IsNullOrEmpty(m_occludersTag) )
			return;
		
		GameObject[] occluderGOs = GameObject.FindGameObjectsWithTag(m_occludersTag);
		
		List<Renderer> occluders = new List<Renderer>();
		foreach( GameObject go in occluderGOs )
		{
			Renderer renderer = go.GetComponent<Renderer>();
			if( renderer != null )
				occluders.Add( renderer );
		}
		
		m_occluders = occluders.ToArray();
	}
	
	private void RenderHighlights( RenderTexture rt)
	{
		if( highlightObjects == null )
			return;

		RenderTargetIdentifier rtid = new RenderTargetIdentifier(rt);
		m_renderBuffer.SetRenderTarget( rtid );


		for(int i = 0; i < highlightObjects.Length; i++)
		{
			if( highlightObjects[i] == null )
				continue;
		
			m_renderBuffer.DrawRenderer( highlightObjects[i], m_highlightMaterial, 0, (int) m_sortingType );
		}

		RenderTexture.active = rt;
		Graphics.ExecuteCommandBuffer(m_renderBuffer);
		RenderTexture.active = null;
	}
	
	private void RenderOccluders( RenderTexture rt)
	{
		if( m_occluders == null )
			return;

		RenderTargetIdentifier rtid = new RenderTargetIdentifier(rt);
		m_renderBuffer.SetRenderTarget( rtid );

		m_renderBuffer.Clear();
		
		foreach(Renderer renderer in m_occluders)
		{	
			m_renderBuffer.DrawRenderer( renderer, m_highlightMaterial, 0, (int) m_sortingType );
		}

		RenderTexture.active = rt;
		Graphics.ExecuteCommandBuffer(m_renderBuffer);
		RenderTexture.active = null;
	}


	/// Final image composing.
	/// 1. Renders all the highlight objects either with Overlay shader or DepthFilter
	/// 2. Downsamples and blurs the result image using standard BlurOptimized image effect
	/// 3. Renders occluders to the same render texture
	/// 4. Substracts the occlusion map from the blurred image, leaving the highlight area
	/// 5. Renders the result image over the main camera's G-Buffer
	private void OnRenderImage( RenderTexture source, RenderTexture destination )
	{
		RenderTexture highlightRT;

		RenderTexture.active = highlightRT = RenderTexture.GetTemporary(m_RTWidth, m_RTHeight, 0, RenderTextureFormat.R8 );
		GL.Clear(true, true, Color.clear);
		RenderTexture.active = null;

		ClearCommandBuffers();

		RenderHighlights(highlightRT);

		RenderTexture blurred = RenderTexture.GetTemporary( m_RTWidth, m_RTHeight, 0, RenderTextureFormat.R8 );


		m_blur.OnRenderImage( highlightRT, blurred );

	
		RenderOccluders(highlightRT);

		if( m_fillType == FillType.Outline )
		{
			RenderTexture occluded = RenderTexture.GetTemporary( m_RTWidth, m_RTHeight, 0, RenderTextureFormat.R8);

			// Excluding the original image from the blurred image, leaving out the areal alone
			m_highlightMaterial.SetTexture("_OccludeMap", highlightRT);
			Graphics.Blit( blurred, occluded, m_highlightMaterial, 2 );

			m_highlightMaterial.SetTexture("_OccludeMap", occluded);

			RenderTexture.ReleaseTemporary(occluded);

		}
		else
		{
			m_highlightMaterial.SetTexture("_OccludeMap", blurred);
		}

		m_highlightMaterial.SetColor("_Color", m_highlightColor);
		Graphics.Blit (source, destination, m_highlightMaterial, (int) m_selectionType);


		RenderTexture.ReleaseTemporary(blurred);
		RenderTexture.ReleaseTemporary(highlightRT);
	}

    public void RefreshHighlight()
    {
        //CreateBuffers();
        //CreateMaterials();
        SetOccluderObjects();

        //m_blur = gameObject.AddComponent<BlurOptimized>();
        //m_blur.enabled = false;

        GameObject[] occludees = GameObject.FindGameObjectsWithTag(m_occludeesTag);
        highlightObjects = new Renderer[occludees.Length];


        for (int i = 0; i < occludees.Length; i++)
            highlightObjects[i] = occludees[i].GetComponent<Renderer>();

        m_RTWidth = (int)(Screen.width / (float)m_resolution);
        m_RTHeight = (int)(Screen.height / (float)m_resolution);
    }
}
