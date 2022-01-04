using System;
using System.IO;
using ImageMagick;
using System.Drawing;
using System.Windows.Forms;

namespace OpenNEF
{
	internal class Program
	{
		static Form ShowLoadingForm()
		{
			Form form = new Form();
			form.Text = "Loading";
			var screen = Screen.PrimaryScreen.Bounds;
			int width = 200;
			int height = 150;
			var bounds = new Rectangle((screen.Width >> 1) - (width >> 1), (screen.Height >> 1) -(height >> 1), width, height);
			form.Refresh();
			form.Bounds = bounds;
			form.StartPosition = FormStartPosition.CenterScreen;
			Label label = new Label();
			int lw = 150;
			int lh = 25;
			label.SetBounds((width >> 1) - (lw >> 1), (height >> 1) - (lh >> 1) - lh, lw, lh);
			label.Text = "Loading NEF file";
			form.FormBorderStyle = FormBorderStyle.None;
			label.Parent = form;

			ProgressBar progress = new ProgressBar();
			int pw = 100;
			int ph = 25;
			progress.SetBounds((width >> 1) - (pw >> 1), (height >> 1) - (ph >> 1) + lh, pw, ph);
			progress.Parent = form;
			progress.UseWaitCursor = true;

			System.Timers.Timer timer = new System.Timers.Timer();
			timer.Interval = 200;
			timer.Elapsed += (s, e) => { if (progress.Value < 100) progress.Value += 10; };
			timer.Start();
			form.FormClosed += (s, e) => { if (timer.Enabled) timer.Stop(); };

			label.TextAlign = ContentAlignment.MiddleCenter;
			System.Threading.Thread thread = new System.Threading.Thread(() => form.ShowDialog());
			thread.Start();
			return form;
		}

		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length == 0) return;

			Form loading = null;
			try
			{
				string file = args[0];
				MemoryStream ms = new MemoryStream();
				loading = ShowLoadingForm();
				MagickImage mimage;
				(mimage = new MagickImage(file, MagickFormat.Nef)).Write(ms, MagickFormat.Png);
				loading.Close();
				Bitmap image = new Bitmap(ms);
				Form form = new Form();
				form.Text = "NEF Image view";
				PictureBox pictureBox = new PictureBox();
				pictureBox.Image = image;
				pictureBox.Parent = form;
				pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
				form.Resize += (s, e) =>
				{
					var size = form.Size;
					pictureBox.SetBounds(0, 0, size.Width, size.Height);
				};
				form.WindowState = FormWindowState.Maximized;
				form.ShowDialog();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				if (loading != null) loading.Close();
			}
		}
	}
}
