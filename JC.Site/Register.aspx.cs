using System;
using System.Web.UI;

namespace JC.Site
{
    public partial class Register : Page
    {
        private readonly Db _db = new Db();

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnRegister_ServerClick(object sender, EventArgs e)
        {
            var cmd = $"insert into tbl_Users(UserName,Email,Password)Values('{txtName.Value}','{txtEmail.Value}','{txtPassword.Value}')";
            var existCmd = $"select * from tbl_Users where Email='{txtEmail.Value}'";

            if (!_db.IsExist(existCmd))
            {
                if (!_db.ExecuteQuery(cmd)) return;

                ScriptManager.RegisterStartupScript(this, GetType(), "Message", "alert('Congratulations!! You have successfully registered..');", true);

                Session["UserName"] = txtName.Value;
                Session["Email"]    = txtEmail.Value;

                Response.Redirect("Chat.aspx");
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "Message", "alert('Email is already Exists!! Please Try Different Email..');", true);
            }
        }
    }
}