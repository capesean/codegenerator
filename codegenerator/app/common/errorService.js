/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .factory("errorService", errorService);
    errorService.$inject = ["notifications"];
    function errorService(notifications) {
        var service = {
            handleApiError: handleApiError
        };
        return service;
        function handleApiError(err, entity, verb) {
            var message = "Failed to " + (verb || "save") + " the " + entity;
            if (err && err.data && err.data[""]) {
                var errors = err.data[""];
                notifications.error(errors.join("<br/>"), message);
            }
            else if (err.data && err.data.modelState) {
                var errs = [];
                for (var key in err.data.modelState) {
                    if (!err.data.modelState.hasOwnProperty(key))
                        continue;
                    errs.push(err.data.modelState[key].join("<br/>"));
                }
                notifications.error(errs.join("<br/>"), message);
            }
            else if (err.data && err.data.exceptionMessage) {
                notifications.error(err.data.exceptionMessage, message, err);
            }
            else if (err.data && err.data.message) {
                notifications.error(err.data.message, message, err);
            }
            else if (err.data && Object.prototype.toString.call(err.data) === "[object Object]") {
                var errs = [];
                for (var key in err.data) {
                    if (!err.data.hasOwnProperty(key))
                        continue;
                    errs.push(err.data[key]);
                }
                notifications.error(errs.join("<br/>"), message);
            }
            else if (err.data) {
                notifications.error(err.data, message, err);
            }
            else if (err.message) {
                notifications.error(err.message, message, err);
            }
            else {
                notifications.error(message + ". " + (err.data && err.data.message ? err.data.message : ""), "Error", err);
            }
        }
    }
    ;
}());
//# sourceMappingURL=errorService.js.map