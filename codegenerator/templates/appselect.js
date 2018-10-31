// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";
    angular
        .module("app")
        .directive("appSelectNAME", appSelectNAME);
    function appSelectNAME() {
        return {
            templateUrl: "/app/directives/appselectNAME_LOWER.html",
            restrict: "E",
            controller: appSelectNAMEController,
            replace: true,
            scope: {
                multiple: "<",
                NAME_LOWER: "=",
                ngModel: "=",
                placeholder: "@",
                singular: "@",
                plural: "@"
            }
        };
    }
    appSelectNAMEController.$inject = ["$scope", "$uibModal"];
    function appSelectNAMEController($scope, $uibModal) {
        $scope.selectNAME = selectNAME;
        $scope.placeholder = $scope.placeholder || ("Select " + $scope.singular.toLowerCase());
        function selectNAME() {
            var modalInstance = $uibModal.open({
                templateUrl: "/app/directives/selectNAME_LOWERmodal.html",
                controller: "selectNAMEModal",
                controllerAs: "vm",
                resolve: {
                    options: function () {
                        return {
                            multiple: $scope.multiple,
                            NAME_LOWER: $scope.NAME_LOWER,
                            singular: $scope.singular,
                            plural: $scope.plural
                        };
                    },
                }
            });
            modalInstance.result.then(function (NAME_LOWER) {
                $scope.NAME_LOWER = NAME_LOWER;
                $scope.ngModel = NAME_LOWER ? NAME_LOWER.NAME_LOWERId : undefined;
            }, function (reason) {
                // cancelled/closed
            });
        }
    }
})();
//# sourceMappingURL=appselect.js.map