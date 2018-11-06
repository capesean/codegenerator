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
                placeholder: "@",
                singular: "@",
                plural: "@"
            }
        }
    }

    appSelectNAMEController.$inject = ["$scope", "$uibModal"];
    function appSelectNAMEController($scope, $uibModal) {

        $scope.selectNAME = selectNAME;
        $scope.placeholder = $scope.placeholder || ("Select " + $scope.singular.toLowerCase());

        function selectNAME() {
            var modalInstance = $uibModal.open({
                templateUrl: "/app/directives/selectNAME_TOLOWERmodal.html",
                controller: "selectNAMEModal",
                controllerAs: "vm",
                size: "xl",
                resolve: {
                    options: () => {
                        return {
                            multiple: $scope.multiple,
                            CAMELCASENAME: $scope.CAMELCASENAME,
                            singular: $scope.singular,
                            plural: $scope.plural
                        }
                    },
                }
            });

            modalInstance.result.then(
                function (CAMELCASENAME) {
                    $scope.CAMELCASENAME = CAMELCASENAME;
                    $scope.ngModel = CAMELCASENAME ? CAMELCASENAME.CAMELCASENAMEId : undefined;
                },
                function (reason) {
                    // cancelled/closed
                });
        }
    }

})();