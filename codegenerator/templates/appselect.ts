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
                plural: "@",
                removeFilters: "<",
                showAddNew: "<"/*FILTER_ATTRIBUTES*/
            }
        }
    }

    appSelectNAMEController.$inject = ["$scope", "$uibModal"];
    function appSelectNAMEController($scope, $uibModal) {

        $scope.selectNAME = selectNAME;
        $scope.placeholder = $scope.placeholder || ("Select " + $scope.singular.toLowerCase());
        $scope.getValue = () => {
            if (!$scope.CAMELCASENAME) return undefined;
            if ($scope.multiple) {
                if ($scope.CAMELCASENAME.length == 0)
                    return undefined;
                var value = "";
                angular.forEach($scope.CAMELCASENAME, CAMELCASENAME => {
                    if (value !== "") value += ", ";
                    value += CAMELCASENAME.LABELFIELD;
                })
                return value;
            }
            return $scope.CAMELCASENAME.LABELFIELD;
        }
        /*FILTER_WATCHES*/
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
                            plural: $scope.plural,
                            removeFilters: $scope.removeFilters,
                            showAddNew: $scope.showAddNew/*FILTER_OPTIONS*/
                        }
                    }
                }
            });

            modalInstance.result.then(
                function (CAMELCASENAME) {
                    $scope.CAMELCASENAME = CAMELCASENAME;
                    if ($scope.multiple) {
                        $scope.ngModel = [];
                        angular.forEach(CAMELCASENAME, item => {
                            $scope.ngModel.push(item.CAMELCASENAMEId);
                        });
                    } else {
                        $scope.ngModel = CAMELCASENAME ? CAMELCASENAME.CAMELCASENAMEId : undefined;
                    }
                },
                function () {
                    // cancelled/closed (reason param)
                }
            );
        }
    }

})();