
function SetCookie(UserName, Password, Id) {
    document.cookie = "Authenticate=" + UserName + ":" + Password + "#" + Id + "; ";
}

function DeleteCookie() {
    document.cookie = "Authenticate=;expires=Thu, 01 Jan 1970 00:00:00 UTC";
}

function GetCookie2(name) {
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; ++i) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            var tmp = c.substring(name.length, c.length);
            var sep = tmp.indexOf('=');
            var val = tmp.substring(sep + 1);
            return val;
        }
    }
    return "";
}

function GetCookie() {
    var name = "Authenticate=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; ++i) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            var tmp = c.substring(name.length, c.length);
            var sep = tmp.indexOf(':');
            var sep2 = tmp.indexOf('#');
            var username = tmp.substring(0, sep);
            var password = tmp.substring(sep + 1, sep2);
            var id = tmp.substring(sep2 + 1);
            var au = new auth(username, password, id);
            return au;
        }
    }
    return "";
}

function IsExist() {
    var user = GetCookie("Authenticate");
    if (user != "") {
        return true;
    }
    else
        return false;
}


function auth(UserName, Password, Id) {
    this.username = UserName;
    this.psw = Password;
    this.id = Id;
}