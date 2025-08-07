using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Tiff3DViewer.Models;
using Tiff3DViewer.Services;

namespace Tiff3DViewer
{
    public partial class MainForm : Form
    {
        private Button btnOpen, btnConvert, btnGenerateMesh, btnExportPLY;
        private TrackBar pageSlider;
        private PictureBox pictureBox;
        private TabControl tabControl;
        private TabPage tab2D, tabPointCloud, tabMesh;
        private List<Bitmap> tiffPages = new List<Bitmap>();
        private List<Point3D> pointCloud = new List<Point3D>();
        private List<MeshTriangle> meshTriangles = new List<MeshTriangle>();
        private GLControl glControl;
        private GLControl glMeshControl;
        private Bitmap currentImage;

        private bool isLoaded = false;
        private float zoom = -300f;
        private float rotX = 0f, rotY = 0f;
        private Point lastMousePos;
        private bool isDragging = false;

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Tiff3DConverter";
            this.Size = new Size(1280, 800);

            btnOpen = new Button { Text = "Open TIFF", Location = new Point(20, 20), Width = 100 };
            btnConvert = new Button { Text = "To Point Cloud", Location = new Point(130, 20), Width = 120 };
            btnGenerateMesh = new Button { Text = "To Mesh", Location = new Point(260, 20), Width = 100 };
            btnExportPLY = new Button { Text = "Export .PLY", Location = new Point(370, 20), Width = 100 };

            btnOpen.Click += BtnOpen_Click;
            btnConvert.Click += BtnConvert_Click;
            btnGenerateMesh.Click += BtnGenerateMesh_Click;
            btnExportPLY.Click += BtnExportPLY_Click;

            this.Controls.AddRange(new Control[] { btnOpen, btnConvert, btnGenerateMesh, btnExportPLY });

            pageSlider = new TrackBar { Location = new Point(480, 20), Width = 300, Minimum = 0, Maximum = 10 };
            pageSlider.Scroll += PageSlider_Scroll;
            this.Controls.Add(pageSlider);

            tabControl = new TabControl { Location = new Point(20, 60), Size = new Size(1220, 680) };
            tab2D = new TabPage("2D Viewer");
            tabPointCloud = new TabPage("Point Cloud Viewer");
            tabMesh = new TabPage("Mesh Viewer");

            pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            tab2D.Controls.Add(pictureBox);

            glControl = new GLControl();
            glControl.Dock = DockStyle.Fill;
            glControl.BackColor = Color.Black;
            glControl.Load += GlControl_Load;
            glControl.Paint += GlControl_Paint;
            glControl.Resize += GlControl_Resize;
            glControl.MouseDown += GlControl_MouseDown;
            glControl.MouseUp += GlControl_MouseUp;
            glControl.MouseMove += GlControl_MouseMove;
            glControl.MouseWheel += GlControl_MouseWheel;
            tabPointCloud.Controls.Add(glControl);

            glMeshControl = new GLControl();
            glMeshControl.Dock = DockStyle.Fill;
            glMeshControl.BackColor = Color.Black;
            glMeshControl.Load += GlMeshControl_Load;
            glMeshControl.Paint += GlMeshControl_Paint;
            glMeshControl.Resize += GlMeshControl_Resize; 
            glMeshControl.MouseClick += GlMeshControl_MouseClick;
            glMeshControl.MouseDown += GlControl_MouseDown;
            glMeshControl.MouseUp += GlControl_MouseUp;
            glMeshControl.MouseMove += GlControl_MouseMove;
            glMeshControl.MouseWheel += GlControl_MouseWheel;

            tabMesh.Controls.Add(glMeshControl);

            tabControl.TabPages.AddRange(new TabPage[] { tab2D, tabPointCloud, tabMesh });
            this.Controls.Add(tabControl);
        }
        private MeshTriangle FindClosestTriangle(int x, int y)
        {
            return meshTriangles.Count > 0 ? meshTriangles[0] : null;
        }
        private float CalculateTriangleVolume(MeshTriangle tri)
        {
            // Signed volume of tetrahedron formed by triangle and origin
            var a = tri.A;
            var b = tri.B;
            var c = tri.C;

            float v = Math.Abs(
                a.X * (b.Y * c.Z - b.Z * c.Y) -
                a.Y * (b.X * c.Z - b.Z * c.X) +
                a.Z * (b.X * c.Y - b.Y * c.X)
            ) / 6f;

            return v;
        }

        private float Distance(Point3D p1, Point3D p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            float dz = p1.Z - p2.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private float CalculateLongestEdge(MeshTriangle tri)
        {
            float ab = Distance(tri.A, tri.B);
            float bc = Distance(tri.B, tri.C);
            float ca = Distance(tri.C, tri.A);
            return Math.Max(ab, Math.Max(bc, ca));
        }

        private float CalculateShortestEdge(MeshTriangle tri)
        {
            float ab = Distance(tri.A, tri.B);
            float bc = Distance(tri.B, tri.C);
            float ca = Distance(tri.C, tri.A);
            return Math.Min(ab, Math.Min(bc, ca));
        }

        private void GlMeshControl_MouseClick(object sender, MouseEventArgs e)
        {
            var mouseX = e.X;
            var mouseY = glMeshControl.Height - e.Y; 

            MeshTriangle hit = FindClosestTriangle(mouseX, mouseY);

            if (hit != null)
            {
                float volume = CalculateTriangleVolume(hit);
                float longAxis = CalculateLongestEdge(hit);
                float shortAxis = CalculateShortestEdge(hit);

                MessageBox.Show($"Volume: {volume}\nLong Axis: {longAxis}\nShort Axis: {shortAxis}",
                                "Triangle Info",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
        }

        private void GlMeshControl_Resize(object sender, EventArgs e)
        {
            if (!glMeshControl.Context.IsCurrent)
                glMeshControl.MakeCurrent();

            GL.Viewport(0, 0, glMeshControl.Width, glMeshControl.Height);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f),
                glMeshControl.Width / (float)glMeshControl.Height,
                1f,
                10000f
            );

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "TIFF Files|*.tif;*.tiff" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tiffPages = TiffLoader.LoadTiffPages(ofd.FileName);
                if (tiffPages.Count > 0)
                {
                    pageSlider.Maximum = tiffPages.Count - 1;
                    pageSlider.Value = 0;
                    pictureBox.Image = tiffPages[0];
                }
                else
                {
                    MessageBox.Show("No pages found or invalid TIFF file.");
                }
            }
        }

        private void BtnConvert_Click(object sender, EventArgs e)
        {
            if (tiffPages == null || tiffPages.Count == 0)
            {
                MessageBox.Show("Please load a TIFF file first.");
                return;
            }

            pointCloud.Clear();
            for (int i = 0; i < tiffPages.Count; i++)
            {
                Bitmap bmp = tiffPages[i];
                var pagePoints = PointCloudBuilder.FromBitmap(bmp, i); // Depth = i
                pointCloud.AddRange(pagePoints);
            }

            glControl.Focus();
            glControl.Invalidate();

            MessageBox.Show($"3D point cloud generated from {tiffPages.Count} pages with total {pointCloud.Count} points.");
        }
        private void BtnGenerateMesh_Click(object sender, EventArgs e)
        {
            if (pointCloud == null || pointCloud.Count == 0)
            {
                MessageBox.Show("Point cloud is empty. Generate it first.");
                return;
            }

            meshTriangles = MeshBuilder.FromPointCloud(pointCloud);

            if (meshTriangles.Count == 0)
            {
                MessageBox.Show("Mesh generation failed or returned no triangles.");
                return;
            }

            tabControl.SelectedTab = tabMesh;
            glMeshControl.Focus();
            glMeshControl.Invalidate();

            MessageBox.Show($"Mesh generated with {meshTriangles.Count} triangles.");
        }

        private void BtnExportPLY_Click(object sender, EventArgs e)
        {
            if (meshTriangles == null || meshTriangles.Count == 0)
            {
                MessageBox.Show("No mesh data to export. Generate the mesh first.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PLY files|*.ply",
                Title = "Save Mesh as PLY",
                FileName = "mesh_output.ply"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            var vertices = new List<Point3D>();
            var indices = new List<int>();

            foreach (var tri in meshTriangles)
            {
                int startIndex = vertices.Count;
                vertices.Add(tri.A);
                vertices.Add(tri.B);
                vertices.Add(tri.C);
                indices.Add(startIndex);
                indices.Add(startIndex + 1);
                indices.Add(startIndex + 2);
            }

            using (var writer = new System.IO.StreamWriter(sfd.FileName))
            {
                writer.WriteLine("ply");
                writer.WriteLine("format ascii 1.0");
                writer.WriteLine($"element vertex {vertices.Count}");
                writer.WriteLine("property float x");
                writer.WriteLine("property float y");
                writer.WriteLine("property float z");
                writer.WriteLine("property uchar red");
                writer.WriteLine("property uchar green");
                writer.WriteLine("property uchar blue");
                writer.WriteLine($"element face {indices.Count / 3}");
                writer.WriteLine("property list uchar int vertex_indices");
                writer.WriteLine("end_header");

                // Write vertices
                foreach (var v in vertices)
                    writer.WriteLine($"{v.X} {v.Y} {v.Z} {v.Color.R} {v.Color.G} {v.Color.B}");

                // Write faces
                for (int i = 0; i < indices.Count; i += 3)
                    writer.WriteLine($"3 {indices[i]} {indices[i + 1]} {indices[i + 2]}");
            }

            MessageBox.Show("PLY file exported successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PageSlider_Scroll(object sender, EventArgs e)
        {
            int index = pageSlider.Value;
            if (index >= 0 && index < tiffPages.Count)
                pictureBox.Image = tiffPages[index];
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
            isLoaded = true;
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            if (!glControl.Context.IsCurrent)
                glControl.MakeCurrent();

            GL.Viewport(0, 0, glControl.Width, glControl.Height);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f),
                glControl.Width / (float)glControl.Height,
                1f,
                10000f
            );

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (!isLoaded || pointCloud == null || pointCloud.Count == 0)
                return;

            glControl.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.LoadIdentity();

            GL.Translate(0f, 0f, zoom);
            GL.Rotate(rotX, 1.0f, 0.0f, 0.0f);
            GL.Rotate(rotY, 0.0f, 1.0f, 0.0f);

            GL.PointSize(2.0f);
            GL.Begin(PrimitiveType.Points);

            foreach (var point in pointCloud)
            {
                Color color = point.Color;
                if (color == Color.Black) color = Color.White;
                GL.Color3(color);
                GL.Vertex3(point.X, point.Y, point.Z);
            }

            GL.End();
            glControl.SwapBuffers();
        }

        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            lastMousePos = e.Location;
        }

        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                rotY += (e.X - lastMousePos.X) * 0.5f;
                rotX += (e.Y - lastMousePos.Y) * 0.5f;
                lastMousePos = e.Location;
                glControl.Invalidate();
            }
        }

        private void GlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            zoom += e.Delta * 0.5f;
            glControl.Invalidate();
        }

        private void GlMeshControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
        }

        private void GlMeshControl_Paint(object sender, PaintEventArgs e)
        {
            if (meshTriangles == null || meshTriangles.Count == 0)
                return;

            glMeshControl.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.LoadIdentity();

            GL.Translate(0f, 0f, zoom);
            GL.Rotate(rotX, 1.0f, 0.0f, 0.0f);
            GL.Rotate(rotY, 0.0f, 1.0f, 0.0f);

            GL.Begin(PrimitiveType.Triangles);
            foreach (var tri in meshTriangles)
            {
                GL.Color3(tri.Color);
                GL.Vertex3(tri.A.X, tri.A.Y, tri.A.Z);
                GL.Vertex3(tri.B.X, tri.B.Y, tri.B.Z);
                GL.Vertex3(tri.C.X, tri.C.Y, tri.C.Z);
            }
            GL.End();

            glMeshControl.SwapBuffers();
        }

    }
}
