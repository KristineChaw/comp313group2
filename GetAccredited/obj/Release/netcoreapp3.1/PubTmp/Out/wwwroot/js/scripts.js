function validatePassword() {
    var password = document.getElementById("Password"),
        confirm_password = document.getElementById("confirm_password");

    if (password.value != confirm_password.value)
        confirm_password.setCustomValidity("Passwords don't match");
    else
        confirm_password.setCustomValidity('');
}

function validateTimePeriod() {
    var from = document.getElementById("Start"),
        to = document.getElementById("End");

    if (from.value != "" && to.value != "") {
        var fromAsDate = new Date(), toAsDate = new Date();
        fromAsDate.setHours(from.value.split(':')[0], from.value.split(':')[1], 0);
        toAsDate.setHours(to.value.split(':')[0], to.value.split(':')[1], 0);

        if (fromAsDate > toAsDate) {
            to.setCustomValidity("Please specify a time after " + dateToString(fromAsDate));
        } else {
            to.setCustomValidity('');
        }
    }
}

function dateToString(date) {
    var hours = date.getHours();
    var minutes = date.getMinutes();
    var ampm = hours >= 12 ? 'PM' : 'AM'
    hours = hours % 12;
    hours = hours ? hours : 12;
    minutes = minutes < 10 ? '0' + minutes : minutes;
    return hours + ':' + minutes + ' ' + ampm;
}

function validateExtension(element, type) {
    var imageExtensions = ["jpeg", "jpg", "png"];
    var docExtensions = ["pdf"];
    var validExtensions = type == "img" ? imageExtensions : docExtensions;

    var fileExtension = element.value.split(".")[1];
    if (!validExtensions.includes(fileExtension)) {
        alert("The specified file cannot be uploaded. Only files with the following extensions are allowed: " + validExtensions.toString() + ".");
        element.value = "";
    } else {
        element.nextElementSibling.hidden = false;
    }
}

function eligibilityFileSelected(selected) {
    var textArea = document.getElementById("Accreditation_Eligibility");
    textArea.disabled = selected;
    textArea.value = (selected == true ? "N/A" : "");
}

function removeFileSelected(element) {
    var fileInput = document.getElementById("Eligibility");
    fileInput.value = "";
    element.hidden = true;

    eligibilityFileSelected(false);
}

function deleteAccreditationFile(accreditationId) {
    if (confirm("You may have unsaved changes that will be lost if you decide to proceed with the deletion.\n\nAre you sure you want to permanently delete this file from GetAccredited?")) {
        window.location.href = "/Accreditation/DeleteRequirementsFile?accreditationId=" + accreditationId;
    }
}

function deleteOrganization(organizationId) {
    if (confirm("You are about to permanently delete an organization.\n\nAll appointments, accreditations, and representatives associated with this organization shall also be deleted.\n\nClick \"OK\" to proceed with the deletion.")) {
        window.location.href = "/Organization/Delete?organizationId=" + organizationId;
    }
}

function deleteRepresentative(repEmail) {
    if (confirm("You are about to remove a representative from an organization.\n\nThe removal will result to the permanent deletion of the representative account.\n\nClick \"OK\" to proceed.")) {
        window.location.href = "/Organization/RemoveRepresentative?email=" + repEmail;
    }
}

function deleteAccreditation(accreditationId) {
    if (confirm("You are about to permanently delete an accreditation.\n\nIf the accreditation has an eligibility requirements file, that file will be deleted as well.\n\nClick \"OK\" to proceed with the deletion.")) {
        window.location.href = "/Accreditation/Delete?accreditationId=" + accreditationId;
    }
}

function deleteDocument(uploadId) {
    if (confirm("Are you sure you want to delete this document? Once deleted, a document cannot be recovered.\n\nClick \"OK\" to proceed.")) {
        window.location.href = "/Account/DeleteStudentFile?uploadId=" + uploadId.toString();
    }
}

function deleteAccount() {
    if (confirm("Are you sure that you want to delete your account? Account deletions are permanent and cannot be undone. All the files you have uploaded shall be deleted as well.\n\nClick \"OK\" to proceed.")) {
        window.location.href = "/Account/Delete";
    }
}