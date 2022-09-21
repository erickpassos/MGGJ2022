using Quantum;
using UnityEngine;
using System;

public class WaterSampler : MonoBehaviour
{
  public Transform Anchor;
  public float CameraOffsetDistance = 10;
  public Transform Camera;

  public AssetRefWaveSample QuantumSample;

  public bool GenerateMesh = false;
  public bool CircularMesh = false;
  public int Side = 5;
  public float Extent = 0.5f;

  //public Vector2 TestOffset;

  public bool UpdateMesh = false;
  public bool UseQuantumWaves = false;

  private Vector3[] _vertices;
  private MeshFilter _filter;
  private Renderer _renderer;
  private int _xIndex;
  private int _zIndex;
  private int _timeIndex;

  // when networked, these are cached
  private bool _initializedForNetwork = false;
  private QuantumGame Game;

  void LateUpdate()
  {
    if (Anchor != null)
    {
      var anchorPos = Anchor.position;
      if (Camera != null)
      {
        var cameraDirection = Camera.forward;
        cameraDirection.y = 0;
        anchorPos += cameraDirection.normalized * CameraOffsetDistance;
      }

      int size = (int)(Extent * 2);
      var x = (((int)anchorPos.x) / size) * size; // + AnchorOffset;
      var z = (((int)(anchorPos.z) / size)) * size; // + AnchorOffset;
      transform.position = new Vector3(x, 0, z);
      _renderer.material.SetFloat(_xIndex, x);
      _renderer.material.SetFloat(_zIndex, z);
    }

    float time = Time.time;

    if (_initializedForNetwork == false)
    {
      Game = QuantumRunner.Default?.Game;
      if (Game != null && Game.Frames.Verified != null) InitWithQuantum();
    }
    else
    {
      var f = Game.Frames.PredictedPrevious;
      time = f.Number * f.DeltaTime.AsFloat + Game.InterpolationFactor * f.DeltaTime.AsFloat;
    }

    _renderer.material.SetFloat(_timeIndex, time);

    // SLOW, for debugging only
    if (UpdateMesh == false) return;
    for (int i = 0; i < _vertices.Length; i++)
    {
      var vertex = _vertices[i];
      var sampleVertex = transform.TransformPoint((vertex));
      //sampleVertex.y = vertex.y = QuantumSample.GetHeight(new Vector2(sampleVertex.x, sampleVertex.z), Time.time);
      _vertices[i] = vertex;
    }

    _filter.mesh.vertices = _vertices;
    _filter.mesh.RecalculateNormals();
  }

  private void InitWithQuantum()
  {
    if (UseQuantumWaves == false) return;
    var sample = QuantumRunner.Default.Game.Frames.Verified.Assets.WaveSample(QuantumSample);
    _renderer.material.SetFloat("_AmplitudeA", sample.WaveSources[0].Amplitude.AsFloat);
    _renderer.material.SetFloat("_SpeedA", sample.WaveSources[0].Speed.AsFloat);
    _renderer.material.SetFloat("_WaveLengthA", sample.WaveSources[0].Wavelength.AsFloat);
    _renderer.material.SetFloat("_AmplitudeB", sample.WaveSources[1].Amplitude.AsFloat);
    _renderer.material.SetFloat("_SpeedB", sample.WaveSources[1].Speed.AsFloat);
    _renderer.material.SetFloat("_WaveLengthB", sample.WaveSources[1].Wavelength.AsFloat);
    _renderer.material.SetFloat("_AmplitudeC", sample.WaveSources[2].Amplitude.AsFloat);
    _renderer.material.SetFloat("_SpeedC", sample.WaveSources[2].Speed.AsFloat);
    _renderer.material.SetFloat("_WaveLengthC", sample.WaveSources[2].Wavelength.AsFloat);
    _initializedForNetwork = true;
  }

  private void Start()
  {
    _filter = GetComponent<MeshFilter>();
    if (GenerateMesh) GeneratePlaneMesh(Side, Extent, _filter.mesh);
    _vertices = _filter.mesh.vertices;
    _renderer = GetComponent<Renderer>();

    _xIndex = _renderer.material.shader.FindPropertyIndex("_X");
    _xIndex = _renderer.material.shader.GetPropertyNameId(_xIndex);
    _zIndex = _renderer.material.shader.FindPropertyIndex("_Z");
    _zIndex = _renderer.material.shader.GetPropertyNameId(_zIndex);
    _timeIndex = _renderer.material.shader.FindPropertyIndex("_RenderTime");
    _timeIndex = _renderer.material.shader.GetPropertyNameId(_timeIndex);
  }

  void GeneratePlaneMesh(int count, float extent, Mesh mesh)
  {
    var half = count / 2;
    var size = extent * 2;
    Vector3[] vertices = new Vector3[count * count * 4 * 3];
    Vector3[] normals = new Vector3[count * count * 4 * 3];
    int[] tris = new int[count * count * 4 * 3];
    Vector2[] UVs = new Vector2[count * count * 4 * 3];
    int vertexIndex = 0;
    for (int i = half; i > -half; i--)
    {
      for (int j = half; j > -half; j--)
      {
        var a = new Vector3(j * size - extent, 0, i * size + extent);
        var b = new Vector3(j * size + extent, 0, i * size + extent);
        var c = new Vector3(j * size - extent, 0, i * size - extent);
        var d = new Vector3(j * size + extent, 0, i * size - extent);
        var center = new Vector3(j * size, 0, i * size);

        if (center.magnitude > half * size && CircularMesh) continue;

        CreateTri(a, b, center, ref vertexIndex, vertices, UVs);
        CreateTri(a, center, c, ref vertexIndex, vertices, UVs);
        CreateTri(b, d, center, ref vertexIndex, vertices, UVs);
        CreateTri(center, d, c, ref vertexIndex, vertices, UVs);
      }
    }

    //Debug.LogFormat("Generated {0} triangles", (vertexIndex / 3));
    if (CircularMesh)
    {
      Array.Resize(ref vertices, vertexIndex);
      Array.Resize(ref normals, vertexIndex);
      Array.Resize(ref UVs, vertexIndex);
      Array.Resize(ref tris, vertexIndex);
    }

    // tris are sequential (low poly look)
    for (int t = 0; t < tris.Length; t++)
    {
      tris[t] = t;
    }

    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    mesh.SetVertices(vertices);
    mesh.SetNormals(normals);
    mesh.SetUVs(0, UVs);
    mesh.triangles = tris;
    // normals are computed here
    mesh.RecalculateNormals();
  }

  void CreateTri(Vector3 a, Vector3 b, Vector3 c, ref int index, Vector3[] vertices, Vector2[] UVs)
  {
    var center = (a + b + c) / 3;
    var uv = new Vector2(center.x, center.z);
    UVs[index] = uv; // - new Vector2(a.x, a.z);
    vertices[index++] = a;
    UVs[index] = uv; // - new Vector2(b.x, b.z);
    vertices[index++] = b;
    UVs[index] = uv; // - new Vector2(c.x, c.z);
    vertices[index++] = c;
  }
}