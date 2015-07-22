using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

//model references for EF
using comp2007_lesson10_wed.Models;
using System.Web.ModelBinding;

namespace comp2007_lesson10_wed
{
    public partial class student : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if save wasn't clicked AND we have a StudentID in the url
            if ((!IsPostBack) && (Request.QueryString.Count > 0))
            {
                GetStudent();
            }
        }

        protected void GetStudent()
        {
            //populate form with existing student record
            Int32 StudentID = Convert.ToInt32(Request.QueryString["StudentID"]);

            //connect to db via EF
            try
            {
                using (comp2007Entities db = new comp2007Entities())
                {
                    //populate a student instance with the StudentID from the URL parameter
                    Student s = (from objS in db.Students
                                 where objS.StudentID == StudentID
                                 select objS).FirstOrDefault();

                    //map the student properties to the form controls if we found a match
                    if (s != null)
                    {
                        txtLastName.Text = s.LastName;
                        txtFirstMidName.Text = s.FirstMidName;
                        txtEnrollmentDate.Text = s.EnrollmentDate.ToString("yyyy-MM-dd");
                    }

                    //enrollments - this code goes in the same method that populates the student form but below the existing code that's already in GetStudent()              
                    var objE = (from en in db.Enrollments
                                join c in db.Courses on en.CourseID equals c.CourseID
                                join d in db.Departments on c.DepartmentID equals d.DepartmentID
                                where en.StudentID == StudentID
                                select new { en.EnrollmentID, en.Grade, c.Title, d.Name });

                    grdCourses.DataSource = objE.ToList();
                    grdCourses.DataBind();
                }
            }
            catch(Exception e)
            {
                Response.Redirect("~/ErrorPage.aspx?handler=Application_Error%20-%20Global.asax");
            }

        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            //use EF to connect to SQL Server
            try
            {
                using (comp2007Entities db = new comp2007Entities())
                {

                    //use the Student model to save the new record
                    Student s = new Student();
                    Int32 StudentID = 0;

                    //check the querystring for an id so we can determine add / update
                    if (Request.QueryString["StudentID"] != null)
                    {
                        //get the id from the url
                        StudentID = Convert.ToInt32(Request.QueryString["StudentID"]);

                        //get the current student from EF
                        s = (from objS in db.Students
                             where objS.StudentID == StudentID
                             select objS).FirstOrDefault();
                    }

                    s.LastName = txtLastName.Text;
                    s.FirstMidName = txtFirstMidName.Text;
                    s.EnrollmentDate = Convert.ToDateTime(txtEnrollmentDate.Text);

                    //call add only if we have no student ID
                    if (StudentID == 0)
                    {
                        db.Students.Add(s);
                    }

                    //run the update or insert
                    db.SaveChanges();

                    //redirect to the updated students page
                    Response.Redirect("students.aspx");
                }
            }
            catch(Exception ex)
            {
                Response.Redirect("~/ErrorPage.aspx?handler=Application_Error%20-%20Global.asax");
            }
        }

        protected void grdCourses_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            //get selected record id
            Int32 EnrollmentID = Convert.ToInt32(grdCourses.DataKeys[e.RowIndex].Values["EnrollmentID"]);
            try
            {
                using (comp2007Entities db = new comp2007Entities())
                {
                    //get selected record
                    Enrollment objE = (from en in db.Enrollments
                                       where en.EnrollmentID == EnrollmentID
                                       select en).FirstOrDefault();

                    //delete
                    db.Enrollments.Remove(objE);
                    db.SaveChanges();

                    //refresh the data on the page
                    GetStudent();
                }
            }
            catch(Exception en)
            {
                Response.Redirect("~/ErrorPage.aspx?handler=Application_Error%20-%20Global.asax");
            }
        }
    }
}