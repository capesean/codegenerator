/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .factory("settingsResource", settingsResource)
        .factory("userResource", userResource)
        .factory("utilitiesResource", utilitiesResource);

    //#region settings resource
    settingsResource.$inject = ["$resource", "appSettings"];
    function settingsResource($resource, appSettings) {
        return $resource(appSettings.apiServiceBaseUri + appSettings.apiPrefix + "settings");
    }
    //#endregion

    //#region user resource
    userResource.$inject = ["$resource", "appSettings"];
    function userResource($resource, appSettings) {
        return $resource(appSettings.apiServiceBaseUri + appSettings.apiPrefix + "users/:id",
            { id: "@id" },
            {
                profile: {
                    method: "GET",
                    url: appSettings.apiServiceBaseUri + appSettings.apiPrefix + "users/:id/profile"
                }
            });
    }
    //#endregion

    //#region utilities resource
    utilitiesResource.$inject = ["$resource", "appSettings"];
    function utilitiesResource($resource, appSettings) {
        return $resource(appSettings.apiServiceBaseUri + appSettings.apiPrefix + "utilities",
            {},
            {
                multiDeploy: {
                    method: "POST",
                    url: appSettings.apiServiceBaseUri + appSettings.apiPrefix + "utilities/multideploy",
                    isArray: true
                }
            });
    }
    //#endregion

} ());