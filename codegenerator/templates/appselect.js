// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .directive("appSelectNAME", appSelectNAME);
    function appSelectNAME() {
        return {
            templateUrl: "/app/directives/appselectNAME_TOLOWER.html",
            restrict: "E",
            controller: appSelectNAMEController,
            replace: true,
            scope: {
                multiple: "<",
                CAMELCASENAME: "=",
                ngModel: "=",
                disabled: "<",
                placeholder: "@",
                singular: "@",
                plural: "@" /*FILTER_ATTRIBUTES*/
            }
        };
    }
    appSelectNAMEController.$inject = ["$scope", "$uibModal"];
    function appSelectNAMEController($scope, $uibModal) {
        $scope.selectNAME = selectNAME;
        $scope.placeholder = $scope.placeholder || ("Select " + $scope.singular.toLowerCase());
        $scope.getValue = function () {
            if (!$scope.CAMELCASENAME)
                return undefined;
            if ($scope.multiple) {
                if ($scope.CAMELCASENAME.length == 0)
                    return undefined;
                var value = "";
                angular.forEach($scope.CAMELCASENAME, function (CAMELCASENAME) {
                    if (value !== "")
                        value += ", ";
                    value += CAMELCASENAME.LABELFIELD;
                });
                return value;
            }
            return $scope.CAMELCASENAME.LABELFIELD;
        };
        /*FILTER_WATCHES*/
        function selectNAME() {
            var modalInstance = $uibModal.open({
                templateUrl: "/app/directives/selectNAME_TOLOWERmodal.html",
                controller: "selectNAMEModal",
                controllerAs: "vm",
                size: "xl",
                resolve: {
                    options: function () {
                        return {
                            multiple: $scope.multiple,
                            CAMELCASENAME: $scope.CAMELCASENAME,
                            singular: $scope.singular,
                            plural: $scope.plural /*FILTER_OPTIONS*/
                        };
                    }
                }
            });
            modalInstance.result.then(function (CAMELCASENAME) {
                $scope.CAMELCASENAME = CAMELCASENAME;
                $scope.ngModel = CAMELCASENAME ? CAMELCASENAME.CAMELCASENAMEId : undefined;
            }, function (reason) {
                // cancelled/closed
            });
        }
    }
})();
//# sourceMappingURL=appselect.js.map