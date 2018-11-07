// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
(function () {
    "use strict";

    angular
        .module("app")
        .controller("selectNAMEModal", selectNAMEModal);

    selectNAMEModal.$inject = ["$scope", "appSettings", "$uibModalInstance", "notifications", "$q", "CAMELCASENAMEResource", "options"];
    function selectNAMEModal($scope, appSettings, $uibModalInstance, notifications, $q, CAMELCASENAMEResource, options) {

        var vm = this;
        vm.loading = true;
        vm.appSettings = appSettings;
        vm.select = select;
        vm.cancel = cancel;
        vm.search = {};
        vm.runNAMESearch = runNAMESearch;
        vm.options = options;
        vm.selectedItems = options.PLURALNAME_TOCAMELCASE ? angular.copy(options.PLURALNAME_TOCAMELCASE) : [];
        vm.close = close;
        vm.clear = clear;
        vm.isSelected = isSelected;
        vm.selectAll = selectAll;
        vm.options = options;

        init();

        function init() {

            options.singular = options.singular || "NAME";
            options.plural = options.plural || "PLURALNAME";

            vm.search = {/*FILTER_PARAMS*/
                includeEntities: true
            };
            /*FILTER_TRIGGERS*/
            runNAMESearch(0, false);

        }

        function runNAMESearch(pageIndex, dontSetLoading) {

            if (!dontSetLoading) vm.loading = true;
            vm.search.pageIndex = pageIndex;

            var promise =
                CAMELCASENAMEResource.query(
                    vm.search,
                    (data, headers) => {

                        vm.PLURALNAME_TOCAMELCASE = data;
                        vm.CAMELCASENAMEHeaders = JSON.parse(headers("X-Pagination"))

                    },
                    err => {

                        notifications.error("Failed to load the PLURALFRIENDLYNAME_TOLOWER.", "Error", err);
                        vm.cancel();

                    }).$promise;

            if (!dontSetLoading) promise.then(() => vm.loading = false);

            return promise;

        };

        function cancel() {
            $uibModalInstance.dismiss();
        }

        function close() {
            if (!!options.multiple) $uibModalInstance.close(vm.selectedItems);
            else $uibModalInstance.dismiss();
        }

        function clear() {
            $uibModalInstance.close(undefined);
        }

        function select(CAMELCASENAME) {
            if (!!options.multiple) {
                if (isSelected(CAMELCASENAME)) {
                    for (var i = 0; i < vm.selectedItems.length; i++) {
                        if (vm.selectedItems[i].CAMELCASENAMEId == CAMELCASENAME.CAMELCASENAMEId) {
                            vm.selectedItems.splice(i, 1);
                            break;
                        }
                    }
                } else {
                    vm.selectedItems.push(CAMELCASENAME);
                }
            } else {
                $uibModalInstance.close(CAMELCASENAME);
            }
        }

        function isSelected(CAMELCASENAME) {
            return vm.selectedItems.filter(item => item.CAMELCASENAMEId === CAMELCASENAME.CAMELCASENAMEId).length > 0;
        }

        function selectAll() {

            vm.loading = true;

            CAMELCASENAMEResource.query(
                {
                    pageSize: 0
                },
                data => {

                    $uibModalInstance.close(data);

                },
                err => {

                    notifications.error("Failed to load the PLURALFRIENDLYNAME_TOLOWER.", "Error", err);


                }).$promise.then(() => vm.loading = false);
        }
    }

}());
