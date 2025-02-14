using CmnTrace;
using OpenQA.Selenium;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using WEB;
namespace BioBank
{

    [ByXPath("//app-root")]
    [OpenEntryTitleToClick("Accessioning")]
    public class AcessioningEntry : BasePage
    {

        public AcessioningEntry(IWebDriver driver, IWebElement element) : base(driver, element)
        {
        }
      

        #region General Section
        public Button btnAdd => this.FindBy<Button>(By.XPath("//button[@title='Create new']"));
        public EditControl edtBarcode => this.FindBy<EditControl>(By.XPath("//input[@class='form-control barcode-input']"));
        public EditControl edtRequisitionNumber => this.FindBy<EditControl>(By.XPath("//input-text[@formcontrolname='displayRequisition']//input"));
        public EditControl edtExternalNumber => this.FindBy<EditControl>(By.XPath("//input-text[@class='half cen-number']//input"));
        public EditControl edtLotNumber => this.FindBy<EditControl>(By.XPath("//input-text[@formcontrolname='lotNumber']//input"));
        public EditControl edtComment => this.FindBy<EditControl>(By.XPath("//input-text[@formcontrolname='comment']//input"));
        public EditControl edtStudyNumber => this.FindBy<EditControl>(By.XPath("//input-lookup[@formcontrolname='study']//input"));
        #endregion

        #region Patient Subject Section
        public EditControl edtSearchPatient => this.FindBy<EditControl>(By.XPath("//input-lookup[@formcontrolname='subject']//input"));
        public EditControl edtSubjectNumber => this.FindBy<EditControl>(By.XPath("//input-text[@formcontrolname='subjectNumber']//input"));
        public EditControl edtPatientExternalNumber => this.FindBy<EditControl>(By.XPath("//collection-external-number//input-text[@class='half cen-number']//input"));
        public EditControl edtDateOfBirth => this.FindBy<EditControl>(By.XPath("//input-text[@formcontrolname='dob']//input"));
        public EditControl edtAge => this.FindBy<EditControl>(By.XPath("//input-text[@formcontrolname='age']//input"));
        public EditControl edtFullName => this.FindBy<EditControl>(By.XPath("//input-text[@formcontrolname='fullName']//input"));
        public EditControl edtRealId => this.FindBy<EditControl>(By.XPath("//input-text[@formcontrolname='realId']//input"));
        public EditControl edtGender => this.FindBy<EditControl>(By.XPath("//input-lookup[@formcontrolname='genderCode']//input"));
        #endregion

        #region Recipient Section
        public CheckBox cbAutologous => this.FindBy<CheckBox>(By.XPath("//input-checkbox[@formcontrolname='autologous']//input"));
        public EditControl edtSearchRecipient => this.FindBy<EditControl>(By.XPath("//input-lookup[@formcontrolname='recipient']//input"));
        #endregion

        #region Specimens Section
        public Button btnAddSpecimen => this.FindBy<Button>(By.ClassName("add-specimen-btn"));
        public GridControl tblSpecimens => this.FindBy<GridControl>(By.Id("specimensTable"));
        #endregion

        #region Action Buttons
        public Button btnPrintLabels => this.FindBy<Button>(By.XPath("//button[contains(@class, 'btn-print')]"));
        public Button btnSpecimenPreparation => this.FindBy<Button>(By.XPath("//button[contains(@class, 'btn-flask')]"));
        public Button btnCancel => this.FindBy<Button>(By.XPath("//button[@t-cancel]"));
        public Button btnSave => this.FindBy<Button>(By.XPath("//button[@t-save]"));
        public Button btnRelease => this.FindBy<Button>(By.XPath("//button[@t-release]"));
        #endregion

        #region Properties
        public string Barcode
        {
            get => edtBarcode.Value;
            set => edtBarcode.SetValue(value, verify: true);
        }

        public string RequisitionNumber
        {
            get => edtRequisitionNumber.Value;
            set => edtRequisitionNumber.SetValue(value, verify: true);
        }

        public string ExternalNumber
        {
            get => edtExternalNumber.Value;
            set => edtExternalNumber.SetValue(value, verify: true);
        }

        public string StudyNumber
        {
            get => edtStudyNumber.Value;
            set => edtStudyNumber.SetValue(value, verify: true);
        }
        #endregion

      
        #region Methods
        public static void AddOrderUsingBioBank(GENE.Order order, WebBioBank bioBank)
        {
            // Open Accessioning Entry
            var accessioningEntry = bioBank.OpenEntry<BioBank.AcessioningEntry>();

            // Click Add button to create new order
            accessioningEntry.btnAdd.Click();

            // Fill General section
            accessioningEntry.ExternalNumber = order.ExternalNumber;
           // accessioningEntry.StudyNumber = order.StudyNumber;

            //if (!string.IsNullOrEmpty(order.LotNumber))
            //    accessioningEntry.edtLotNumber.SetValue(order.LotNumber);

            if (!string.IsNullOrEmpty(order.Comment))
                accessioningEntry.edtComment.SetValue(order.Comment);

            // Fill Patient Subject section
            accessioningEntry.edtSearchPatient.SelectValueFromLookUp("RANDOM");
            accessioningEntry.WaitForPatientDataLoad();

            // Fill Recipient section if needed
            //if (order.IsAutologous)
            //{
            //    accessioningEntry.chkAutologous.Check();
            //}
            //else if (!string.IsNullOrEmpty(order.RecipientId))
            //{
            //    accessioningEntry.edtSearchRecipient.SetValue(order.RecipientId);
            //    accessioningEntry.WaitForRecipientDataLoad();
            //}

            // Add specimens if any
            if (order.Specimens != null && order.Specimens.Any())
            {
                foreach (var specimen in order.Specimens)
                {
                    accessioningEntry.AddSpecimen(specimen);
                }
            }

            // Save or Release based on order status
            if (order.ShouldRelease)
            {
                accessioningEntry.btnRelease.Click();
            }
            else
            {
                accessioningEntry.btnSave.Click();
            }
            order.RequisitionNumber= accessioningEntry.RequisitionNumber;

            // Wait for operation to complete
            accessioningEntry.WaitForSaveComplete();
        }

        private void WaitForPatientDataLoad()
        {
            // Wait for patient data to be loaded and displayed
            this.WaitForCondition(() =>
                !string.IsNullOrEmpty(edtFullName.Value) &&
                !string.IsNullOrEmpty(edtSubjectNumber.Value));
        }

        private void WaitForCondition(Func<bool> value)
        {
            throw new NotImplementedException();
        }

        private void WaitForRecipientDataLoad()
        {
            // Wait for recipient lookup to complete
            this.WaitForCondition(() =>
                !edtSearchRecipient.HasClass("loading"));
        }

        private void AddSpecimen(GENE.Specimen specimen)
        {
            btnAddSpecimen.Click();
            // Additional logic for adding specimen details
            // This would depend on the specimen form structure
        }

        private void WaitForSaveComplete()
        {
            // Wait for save/release operation to complete
            this.WaitForCondition(() =>
                !btnSave.Enabled && !btnRelease.Enabled);
        }
        #endregion
    }
}
}