using System;

namespace JC.Site
{
    public partial class Login : System.Web.UI.Page
    {
        private readonly Db _db = new Db();

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnSignIn_Click(object sender, EventArgs e)
        {
            var Query = $"select * from tbl_Users where Email='{txtEmail.Value}' and Password='{txtPassword.Value}'";

            if (_db.IsExist(Query))
            {
                var UserName = _db.GetColumnVal(Query, "UserName");

                Session["UserName"] = UserName;
                Session["Email"] = txtEmail.Value;

                Response.Redirect("Chat.aspx");
            }
            else
                txtEmail.Value = "Invalid Email or Password!!";
        }
    }
}