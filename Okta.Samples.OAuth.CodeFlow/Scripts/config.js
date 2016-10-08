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


(function (root, factory) {
    if (typeof define === 'function' && define.amd) {
        define([], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory();
    } else {
        root.OktaConfig = factory();
    }
}

(this, function () {
    return {
        orgUrl: 'https://oidctest.oktapreview.com',
        clientId: 'Fivx0EHloPtmLyv8LgXc',
        scope: ['openid', 'email', 'phone', 'address', 'groups', 'profile', 'groups'],
        // use the scope value below and comment out the line above if you want to test the 'call-api' custom scope (requires API Access Management SKU)
        // scope: ['openid', 'email', 'phone', 'address', 'groups', 'profile', 'groups', 'call-api'],
        redirectUri: 'http://localhost:8080/Callback',
        // Url of the back-end resource server API
        webApiUrl: 'https://localhost:44301',
        // set the value of callApiWithAT to true if you want to call the Resource Server API with the Access Token as an authentication bearer token
        callApiWithAT: false
    };
}));
