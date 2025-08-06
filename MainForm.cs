using Accord.Math.Geometry;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tiff3DViewer.Models;
using Tiff3DViewer.Services;

namespace Tiff3DViewer
{
    public partial class MainForm : Form
    {
        private Button btnOpen;
        private Button btnConvert;
        private Button btnGenerateMesh;
        private Button btnExportPLY;
        private TrackBar pageSlider;
        private PictureBox pictureBox;
        private TabControl tabControl;
        private TabPage tab2D;
        private TabPage tabPointCloud;
        private TabPage tabMesh;
        private List<Bitmap> tiffPages = new List<Bitmap>();
        private List<Point3D> pointCloud = new List<Point3D>();
        private GLControl glControl;
        private bool isLoaded = false;

        public MainForm()
        {
            InitializeComponent();
            SetupUI();

        }
        private void SetupUI()
        {
            this.Text = "Tiff3DConverter";
            this.Size = new Size(1280, 800);

            btnOpen = new Button() { Text = "Open TIFF", Location = new Point(20, 20), Width = 100 };
            btnOpen.Click += BtnOpen_Click;
            this.Controls.Add(btnOpen);

            btnConvert = new Button() { Text = "To Point Cloud", Location = new Point(130, 20), Width = 120 };
            btnConvert.Click += BtnConvert_Click;
            this.Controls.Add(btnConvert);

            btnGenerateMesh = new Button() { Text = "To Mesh", Location = new Point(260, 20), Width = 100 };
            btnGenerateMesh.Click += BtnGenerateMesh_Click;
            this.Controls.Add(btnGenerateMesh);

            btnExportPLY = new Button() { Text = "Export .PLY", Location = new Point(370, 20), Width = 100 };
            btnExportPLY.Click += BtnExportPLY_Click;
            this.Controls.Add(btnExportPLY);

            pageSlider = new TrackBar() { Location = new Point(480, 20), Width = 300, Minimum = 0, Maximum = 10 };
            pageSlider.Scroll += PageSlider_Scroll;
            this.Controls.Add(pageSlider);

            tabControl = new TabControl() { Location = new Point(20, 60), Size = new Size(1220, 680) };

            tab2D = new TabPage("2D Viewer");
            pictureBox = new PictureBox() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            tab2D.Controls.Add(pictureBox);

            tabPointCloud = new TabPage("Point Cloud Viewer");
            tabMesh = new TabPage("Mesh Viewer");

            tabControl.TabPages.Add(tab2D);
            tabControl.TabPages.Add(tabPointCloud);
            tabControl.TabPages.Add(tabMesh);

            this.Controls.Add(tabControl);
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "TIFF Files|*.tif;*.tiff";

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

            int currentIndex = pageSlider.Value;
            Bitmap currentImage = tiffPages[currentIndex];

            pointCloud = PointCloudBuilder.FromBitmap(currentImage);

            MessageBox.Show($"Point cloud generated with {pointCloud.Count} points.");
        }
        private void BtnGenerateMesh_Click(object sender, EventArgs e) { }
        private void BtnExportPLY_Click(object sender, EventArgs e) { }
        private void PageSlider_Scroll(object sender, EventArgs e)
        {
            int index = pageSlider.Value;
            if (index >= 0 && index < tiffPages.Count)
            {
                pictureBox.Image = tiffPages[index];
            }
        }


    }
}
