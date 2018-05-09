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

        function unwrapException(err) {
            if (err.innerException)
                return unwrapException(err.innerException);
            else
                return err.exceptionMessage || err.message;
        }

        function handleApiError(err, entity, verb) {

            var message = "Failed to " + (verb || "save") + " the " + entity;

            if (err && err.status === 404) {
                notifications.error("The requested " + entity + " could not be found", message);
            } else if (err && err.status === 401) {
                notifications.error("You are not authorized to perform that action", message);
            } else if (err && err.status === 403) {
                notifications.error("You are not permitted to perform that action", message);
            } else if (err && err.status === -1) {
                notifications.error("Unable to communicate with the website.", "Connection Issue");
            } else if (err && err.data && err.data[""]) {
                var errors = err.data[""];
                notifications.error(errors.join("<br/>"), message);
            } else if (err.data && err.data.modelState) {
                var errs = [];
                for (var key in err.data.modelState) {
                    if (!err.data.modelState.hasOwnProperty(key)) continue;
                    errs.push(err.data.modelState[key].join("<br/>"));
                }
                notifications.error(errs.join("<br/>"), message);
            } else if (err.data && err.data.exceptionMessage) {
                notifications.error(err.data.exceptionMessage, message, err);
            } else if (err.data && err.data.innerException) {
                var errText = unwrapException(err.data.innerException);
                notifications.error(errText, message, err);
            } else if (err.data && err.data.message) {
                notifications.error(err.data.message, message, err);
            } else if (err.data && Object.prototype.toString.call(err.data) === "[object Object]") {
                var errs = [];
                for (var key in err.data) {
                    if (!err.data.hasOwnProperty(key)) continue;
                    errs.push(err.data[key]);
                }
                notifications.error(errs.join("<br/>"), message);
            } else if (err.data) {
                notifications.error(err.data, message, err);
            } else if (err.message) {
                notifications.error(err.message, message, err);
            } else {
                notifications.error(message + ". " + (err.data && err.data.message ? err.data.message : ""), "Error", err);
            }
        }
    };

}());