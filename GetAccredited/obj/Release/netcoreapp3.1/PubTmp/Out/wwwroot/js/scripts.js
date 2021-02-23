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