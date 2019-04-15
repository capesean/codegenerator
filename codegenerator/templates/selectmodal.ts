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
        vm.moment = moment;
        vm.select = select;
        vm.cancel = cancel;
        vm.search = {};
        vm.runNAMESearch = runNAMESearch;
        vm.options = options;
        vm.selectedItems = options.CAMELCASENAME ? angular.copy(options.CAMELCASENAME) : [];
        vm.close = close;
        vm.clear = clear;
        vm.isSelected = isSelected;
        vm.selectAll = selectAll;

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

                    }
                ).$promise.catch(
                    err => {

                        notifications.error("Failed to load the PLURALFRIENDLYNAME_TOLOWER.", "Error", err);
                        vm.cancel();

                    }
                );

            if (!dontSetLoading) promise.finally(() => vm.loading = false);

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
            if (options.multiple) $uibModalInstance.close([]);
            else $uibModalInstance.close(undefined);
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
            if (!options.multiple) return false;
            return vm.selectedItems.filter(item => item.CAMELCASENAMEId === CAMELCASENAME.CAMELCASENAMEId).length > 0;
        }

        function selectAll() {

            vm.loading = true;

            vm.search.pageSize = 0;
            vm.search.pageIndex = 0;

            CAMELCASENAMEResource.query(
                vm.search
            ).$promise.then(
                data => {

                    $uibModalInstance.close(data);

                },
                err => {

                    notifications.error("Failed to load the PLURALFRIENDLYNAME_TOLOWER.", "Error", err);

                }
            ).finally(() => vm.loading = false);
        }
    }

}());
