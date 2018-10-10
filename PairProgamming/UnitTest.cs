using System;
using System.Collections.Generic;

//Unit Test
//For DoWork, sendButton_Click, SetText

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

            [Test]
            public void TestDoWork()
            {
                TDoWork controller = new TDoWork();
                controller.Show();

                TextBoxTester textBox = new TextBoxTester("txtName");
                textBox["Text"] = "Hello Wizard";

                ButtonTester button = new ButtonTester("sendButton_Click");
                button.Click();

            }

            [TestCase(new byte[] { 0x00, 0x01, 0x02, 0x03 })]
            public void sample(byte[] input)
            {
                Assert.That(input, Is.EqualTo(new byte[] { 0x00, 0x01, 0x02, 0x03 }));
            }

            [Test]
            public void SimpleTestUsingMessageBox()
            {
 
            WhenCalled(() => MessageBox.Show(String.Empty)).WillReturn(DialogResult.OK);

            MessageBox.Show("This is a message");

            Verify.WasCalledWithExactArguments(() => MessageBox.Show("This is a message"));
            }

        }

    }
}