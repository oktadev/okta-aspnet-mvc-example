/*!
 * Copyright (c) 2016, Okta, Inc. and/or its affiliates. All rights reserved.
 * The Okta software accompanied by this notice is provided pursuant to the Apache License, Version 2.0 (the "License.")
 *
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *
 * See the License for the specific language governing permissions and limitations under the License.
 */


var idTokenKey = 'idToken';
var sessionTokenKey = 'sessionToken';
var userLoginKey = 'userLogin';


function renderOktaWidget() {
    oktaSessionsMe(function (authenticated) {
        console.log('Is user authenticated? ' + authenticated);
        if (!authenticated) {
            showAuthUI(false, "");
            oktaSignIn.renderEl(
                { el: '#okta-sign-in-widget' },
                function (res) {
                    if (res.status === 'SUCCESS') {
                        console.log(res);
                        var id_token = res.idToken;
                        console.log('id token: ' + id_token);
                        sessionStorage.setItem(idTokenKey, id_token);
                        sessionStorage.setItem(userLoginKey, res.claims.preferred_username);
                        showAuthUI(true, res.claims.preferred_username);
                    }
                },
                function (err) { console.log('Unexpected error authenticating user: %o', err); }
            );
        }
        else {
            var id_token = sessionStorage.getItem(idTokenKey);
            if (id_token === null) {
                console.log('calling renewToken');
                callRenewToken();
            }
            var userLogin = sessionStorage.getItem(userLoginKey);
            if (userLogin) {
                console.log('user Login is ' + userLogin);
            }

            showAuthUI(true, userLogin);
        }
    });
}

function showAuthUI(isAuthenticated, user_id) {
    if (isAuthenticated) {
        $("#apicall-buttons").show();
        $('#navbar > ul').empty().append('<li><a id="logout" href="/logout">Sign out</a></li>');
        $('#logout').click(function (event) {
            event.preventDefault();
            signOut();
        });
        $('#okta-sign-in-widget').hide();
        $('#logged-out-message').hide();
        $('#logged-in-message').show();
        if (user_id) {
            $('#okta-user-id').empty().append(user_id);
            $('#logged-in-user-id').show();
        }
    }
    else {
        $("#apicall-buttons").hide();
        $('#navbar > ul').empty();
        $('#logged-in-message').hide();
        $('#logged-out-message').show();
        $('#logged-in-user-id').hide();
        $('#okta-sign-in .okta-form-input-field input[type="password"]').val('');
        console.log('show sign-in widget')
        $('#okta-sign-in-widget').show();
    }
}

function callUnsecureWebApi() {
    $.ajax({
        type: "GET",
        dataType: 'json',
        url: webApiRootUrl + "/unprotected",
        success: function (data) {
            console.log(data);
            $('#logged-in-res').text(data);
        }
    });
}

function callSecureWebApi() {
    $.ajax({
        type: "GET",
        dataType: 'json',
        url: webApiRootUrl + "/protected",
        beforeSend: function (xhr) {
            var id_token = sessionStorage.getItem(idTokenKey);
            console.log("callSecureWebApi ID Token: " + id_token);
            if (id_token) {
                xhr.setRequestHeader("Authorization", "Bearer " + id_token);
            }
        },
        success: function (data) {
            $('#logged-in-res').text(data);
        },
        error: function (textStatus, errorThrown) {
            console.log('error while calling secure web api: ' + errorThrown);
            console.log(textStatus);
            $('#logged-in-res').text("You must be logged in AND have the proper permissions to access this API endpoint");
        }
    });
}

function callUserInfo() {
    $.ajax({
        type: "GET",
        dataType: 'json',
        url: oktaOrgUrl + "/oauth2/v1/userinfo",
        beforeSend: function (xhr) {
            var id_token = sessionStorage.getItem(idTokenKey);
            if (id_token) {
                xhr.setRequestHeader("Authorization", "Bearer " + id_token);
            }
        },
        success: function (data) {
            console.log(data);
        },
        error: function (textStatus, errorThrown) {
            $('#logged-in-res').text("You must be logged in to call this API");
        }
    });
}

function callUsersMe() {
    oktaUsersMe(function (authenticated) {
        console.log('Is user authenticated? ' + authenticated);
        return authenticated;
    });
}

function callSessionsMe() {
    oktaSessionsMe(function (authenticated) {
        console.log('Is user authenticated? ' + authenticated);
        return authenticated;
    });
}

function signOut() {
    console.log('signing out');
    oktaSessionsMe(function (authenticated) {
        if (authenticated) {
            var sessionToken;
            var sessionTokenString = sessionStorage.getItem(sessionTokenKey);
            if (sessionTokenString) {
                sessionToken = JSON.parse(sessionTokenString);
                console.log(sessionToken);
                var sessionId = sessionToken.id;
                console.log('closing session ' + sessionId);
                closeSession(function (success) {
                    console.log('Is session closed? ' + success);
                    if (success) {
                        location.reload(false);
                        renderOktaWidget();
                    }
                })
            }
        }
    });

}

function oktaSessionsMe(callBack) {
    $.ajax({
        type: "GET",
        dataType: 'json',
        url: oktaOrgUrl + "/api/v1/sessions/me",
        xhrFields: {
            withCredentials: true
        },
        success: function (data) {
            console.log('setting success to true');
            console.log("My session: ");
            console.log(data);
            sessionStorage.setItem(sessionTokenKey, JSON.stringify(data));
            return callBack(true);
            //$('#logged-in-res').text(data);
        },
        error: function (textStatus, errorThrown) {
            console.log('setting success to false');
            //$('#logged-in-res').text("You must be logged in to call this API");
            return callBack(false);
        },
        async: true
    });
}

function oktaUsersMe(callBack) {
    $.ajax({
        type: "GET",
        dataType: 'json',
        url: oktaOrgUrl + "/api/v1/users/me",
        xhrFields: {
            withCredentials: true
        },
        success: function (data) {
            console.log(data);
            return callBack(true);
        },
        error: function (textStatus, errorThrown) {
            return callBack(false);
        },
        async: true
    });
}

function closeSession(callback) {
    $.ajax({
        url: oktaOrgUrl + '/api/v1/sessions/me',
        type: 'DELETE',
        xhrFields: { withCredentials: true },
        accept: 'application/json'
    }).done(function (data, textStatus, xhr) {
        console.log('DONE - data = ' + data + ' textStatus = ' + textStatus + ' xhr.status = ' + xhr.status);
        return callback(true);
    }).fail(function (xhr, textStatus, error) {
        console.log('FAILED - error = ' + error + ' textStatus = ' + textStatus + ' xhr.status = ' + xhr.status);
        return callback(false);
    }
               );
}

function callRenewToken() {
    oktaSignIn.idToken.refresh(null, function (token) {
        console.log('New ID token: ', token);
        sessionStorage.setItem(idTokenKey, token);
        sessionStorage.setItem(userLoginKey, token.claims.preferred_username);
        return token;
    });
}
