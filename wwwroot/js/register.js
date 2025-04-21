
        function saveFormData() {
            sessionStorage.setItem("username", document.getElementById("username").value);
            sessionStorage.setItem("email", document.getElementById("email").value);
            sessionStorage.setItem("password", document.getElementById("password").value);
            sessionStorage.setItem("confirmPassword", document.getElementById("confirmPassword").value);
            sessionStorage.setItem("agreeTerms", document.getElementById("agreeTerms").checked);
            
    }

        function loadFormData() {
        if (sessionStorage.getItem("username")) {
            document.getElementById("username").value = sessionStorage.getItem("username");
        }
        if (sessionStorage.getItem("email")) {
            document.getElementById("email").value = sessionStorage.getItem("email");
        }
        if (sessionStorage.getItem("password")) {
            document.getElementById("password").value = sessionStorage.getItem("password");
        }
        if (sessionStorage.getItem("confirmPassword")) {
            document.getElementById("confirmPassword").value = sessionStorage.getItem("confirmPassword");
        }
        if (sessionStorage.getItem("agreeTerms") === "true") {
            document.getElementById("agreeTerms").checked = true;
        }
    }

        function validateCheckbox() {
        if (!document.getElementById("agreeTerms").checked) {
            alert("Ви повинні підтвердити, що ознайомилися з правилами спільноти.");
        return false;
        }
        return true;
}

        


        document.addEventListener("DOMContentLoaded", loadFormData);

