﻿function validatePassword() {
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
        window.location.href = "/Accreditation/DeleteFile?accreditationId=" + accreditationId;
    }
}