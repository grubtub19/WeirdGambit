using System;
using System.Collections.Generic;

//Unit Test Examples 
//Assert ideas for our code
//Not all applicable

namespace WizardOfOz
{
    public class ContainsTests
    {
        public class OzTests
        {
          
            [Test]
            public void TestOnButtonPressed()
            {
                MyUserInterface controller = new MyUserInterface();
                controller.Show();

                TextBoxTester textBox = new TextBoxTester("txtName");
                textBox["Text"] = "Hello Wizard";

                ButtonTester button = new ButtonTester("sendButton_Click");
                button.Click();

            }
   
        }

    }
}