using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Web;
using SD = System.Drawing;

namespace JC.Site
{
    public partial class Chat : System.Web.UI.Page
    {
        public           string UserName         = "admin";
        public           string UserImage        = "/images/DP/dummy.png";
        protected        string UploadFolderPath = "~/Uploads/";
        private readonly Db     _db              = new Db();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserName"] != null)
            {
                UserName = Session["UserName"].ToString();
                GetUserImage(UserName);
            }
            else
                Response.Redirect("Login.aspx");

            Header.DataBind();
        }

        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }

        public void GetUserImage(string Username)
        {
            if (Username == null) return;

            var query = $"select Photo from tbl_Users where UserName='{Username}'";

            var ImageName = _db.GetColumnVal(query, "Photo");

            if (ImageName != "")
                UserImage = $"images/DP/{ImageName}";
        }

        protected void btnChangePicModel_Click(object sender, EventArgs e)
        {
            var serverPath = HttpContext.Current.Server.MapPath("~/");

            if (!FileUpload1.HasFile) return;

            var FileWithPat = $@"{serverPath}images/DP/{UserName}{FileUpload1.FileName}";

            FileUpload1.SaveAs(FileWithPat);

            var img  = Image.FromFile(FileWithPat);
            var img1 = ResizeImage(img, 151, 150);

            img1.Save(FileWithPat);

            if (!File.Exists(FileWithPat)) return;

            var fi        = new FileInfo(FileWithPat);
            var ImageName = fi.Name;
            var query     = $"update tbl_Users set Photo='{ImageName}' where UserName='{UserName}'";

            if (_db.ExecuteQuery(query))
                UserImage = $"images/DP/{ImageName}";
        }


        #region Resize Image With Best Qaulity

        private static Image ResizeImage(Image img, int maxWidth, int maxHeight)
        {
            if (img.Height < maxHeight && img.Width < maxWidth) return img;
            using (img)
            {
                var xRatio = (double) img.Width / maxWidth;
                var yRatio = (double) img.Height / maxHeight;
                var ratio  = Math.Max(xRatio, yRatio);
                var nnx    = (int) Math.Floor(img.Width / ratio);
                var nny    = (int) Math.Floor(img.Height / ratio);
                var cpy    = new Bitmap(nnx, nny, SD.Imaging.PixelFormat.Format32bppArgb);
                using (var gr = Graphics.FromImage(cpy))
                {
                    gr.Clear(Color.Transparent);

                    // This is said to give best quality when resizing images
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    gr.DrawImage(img,
                                 new Rectangle(0, 0, nnx, nny),
                                 new Rectangle(0, 0, img.Width, img.Height),
                                 GraphicsUnit.Pixel);
                }

                return cpy;
            }
        }

        #endregion

        protected void FileUploadComplete(object sender, EventArgs e)
        {
            var filename = Path.GetFileName(AsyncFileUpload1.FileName);

            AsyncFileUpload1.SaveAs(Server.MapPath(UploadFolderPath) + filename);
        }
    }
}