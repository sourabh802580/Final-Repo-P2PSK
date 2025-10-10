﻿// Wait for DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function () {
    const calendarEl = document.getElementById('calendar');

    // Initialize FullCalendar
    const calendar = new FullCalendar.Calendar(calendarEl, {
        height: 'auto',
        stickyHeaderDates: true,
        themeSystem: 'bootstrap5',
        initialView: 'dayGridMonth',
        events: '/Account/GetEvents',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,listMonth,listYear'
        },
        eventClick: function (info) {
            const props = info.event.extendedProps;
            showEventModal(info.event.title, props);
        },
        eventDidMount: function (info) {
            info.el.style.cursor = 'pointer';
        },
        views: {
            dayGridMonth: { buttonText: 'Month' },
            listMonth: { buttonText: 'list month' },
            listYear: { buttonText: 'list year' }
        },
    });

    // Render the calendar
    calendar.render();

    // Function to display event details in modal
    function showEventModal(title, props) {
        const module = props.module;

        // Reset modal content
        const modalTitle = document.querySelector('#eventModal .modal-title');
        const modalBody = document.querySelector('#eventModal .modal-body');
        modalBody.innerHTML = '';

        // Render content based on module type
        switch (module) {
            case "PurchaseRequisition":
                renderPR(props);
                break;
            case "RequestForQuotation":
                renderRFQ(props);
                break;
            case "RegisterQuotation":
                renderRQ(props);
                break;
            case "PurchaseOrder":
                renderPO(props);
                break;
            case "GRNInfo":
                renderGRN(props);
                break;
            case "GoodsReturnInfo":
                renderGoodsReturn(props);
                break;
            case "QualityCheckInfo":
                renderQC(props);
                break;
            default:
                modalBody.innerHTML = '<p>No data available</p>';
        }

        // Show the Bootstrap modal
        const modal = new bootstrap.Modal(document.getElementById('eventModal'));
        modal.show();
    }

    // ----------------------- Helper Functions -----------------------

    // Return value or fallback if undefined/null
    function safe(val) {
        return val ? val : '-';
    }

    // ----------------------- Module Render Functions -----------------------

    // Render Purchase Requisition
    function renderPR(props) {
        if (!props) return;

        const modalTitle = document.querySelector('#eventModal .modal-title');
        modalTitle.textContent = "Purchase Requisition";

        // Build modal HTML
        let html = `
        <div>
            <div class="container">
                <div class="row">
                    <div class="col-sm-6">
                        <strong>PR Code:</strong> ${safe(props.PRCode)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Required Date:</strong> ${safe(props.RequiredDate)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Added By:</strong> ${safe(props.AddedBy)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Added Date:</strong> ${safe(props.AddedDate)}
                    </div>
                    <div class="col-sm-6">
                        <strong>${props.StatusName == 'Rejected' ? 'Rejected By' : 'Approved By'}:</strong> ${safe(props.ApprovedBy)}
                    </div>
                    <div class="col-sm-6">
                        <strong>${props.StatusName == 'Rejected' ? 'Rejected Date' : 'Approved Date'}:</strong> ${safe(props.ApprovedDate)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Status:</strong> ${safe(props.StatusName)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Priority:</strong> ${safe(props.PriorityName)}
                    </div>
                    <div class="col-sm-12 mb-3">
                        <strong>Description:</strong> ${safe(props.Description)}
                    </div>
                </div>
            </div>
            <hr/>
            <table id="prItemsTable" class="table table-striped table-bordered" style="width:100%">
                <thead class="table-dark text-uppercase">
                    <tr>
                        <th>Sr. No.</th>
                        <th class="d-none">PRCode</th>
                        <th class="d-none">PRItemCode</th>
                        <th class="d-none">ItemCode</th>
                        <th>Item Name</th>
                        <th>Required Quantity</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>`;

        document.querySelector('#eventModal .modal-body').innerHTML = html;

        // Initialize DataTable
        if (props.Items && props.Items.length > 0) {
            new DataTable('#prItemsTable', {
                data: props.Items,
                destroy: true,
                ordering: false,
                columns: [
                    {
                        data: null,
                        orderable: false,
                        searchable: false,
                        render: function (data, type, row, meta) {
                            return meta.row + 1
                        }
                    },
                    { data: "PRCode", visible: false },
                    { data: "PRItemCode", visible: false },
                    { data: "ItemCode", visible: false },
                    { data: "ItemName", render: safe },
                    { data: "RequiredQuantity", render: safe }
                ],
                columnDefs: [
                    { className: "text-center", targets: "_all" }
                ],
                language: {
                    lengthMenu: ""
                },
            });
        }
    }

    // Render Request for Quotation
    function renderRFQ(props) {
        if (!props) return;
        const modalTitle = document.querySelector('#eventModal .modal-title');
        modalTitle.textContent = "Request For Quotation";

        // Build modal HTML
        let html = `
        <div>
            <div class="container">
                <div class="row">
                    <div class="col-sm-6">
                        <strong>RFQ Code:</strong> ${safe(props.RFQCode)}
                    </div>
                    <div class="col-sm-6">
                        <strong>PR Code:</strong> ${safe(props.PRCode)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Added By:</strong> ${safe(props.AddedBy)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Added Date:</strong> ${safe(props.AddedDate)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Accountant Name:</strong> ${safe(props.AccountantName)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Accountant Email:</strong> ${safe(props.AccountantEmail)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Expected Date:</strong> ${safe(props.ExpectedDate)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Delivery Address:</strong> ${safe(props.DeliveryAddress)}
                    </div>
                    <div class="col-sm-12 mb-3">
                        <strong>Description:</strong> ${safe(props.Description)}
                    </div>
                </div>
            </div>
            <hr/>
            <table id="rfqItemsTable" class="table table-striped table-bordered" style="width:100%">
                <thead class="table-dark text-uppercase">
                    <tr>
                        <th>Sr. No.</th>
                        <th class="d-none">PRCode</th>
                        <th class="d-none">PRItemCode</th>
                        <th class="d-none">ItemCode</th>
                        <th>Item Name</th>
                        <th>Required Quantity</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        </div>`;

        document.querySelector('#eventModal .modal-body').innerHTML = html;

        // Initialize DataTable
        if (props.Items && props.Items.length > 0) {
            new DataTable('#rfqItemsTable', {
                data: props.Items,
                destroy: true,
                ordering: false,
                columns: [
                    {
                        data: null,
                        orderable: false,
                        searchable: false,
                        render: function (data, type, row, meta) {
                            return meta.row + 1
                        }
                    },
                    { data: "RFQCode", visible: false },
                    { data: "PRItemCode", visible: false },
                    { data: "ItemCode", visible: false },
                    { data: "ItemName", render: safe },
                    { data: "RequiredQuantity", render: safe }
                ],
                columnDefs: [
                    { className: "text-center", targets: "_all" }
                ],
                language: {
                    lengthMenu: ""
                },
            });
        }
    }

    // Render Registered Quotation
    function renderRQ(props) {
        if (!props) return;

        const modalTitle = document.querySelector('#eventModal .modal-title');
        modalTitle.textContent = "Register Quotation";

        let html = `
    <table id="rqTable" class="table table-striped table-bordered" style="width:100%">
        <thead class="table-dark text-uppercase">
            <tr>
                <th>Sr. No.</th>
                <th class="d-none">RegisterQuotationCode</th>
                <th>RFQ Code</th>
                <th>Vendor Name</th>
                <th>Status</th>
                <th>Added By</th>
                <th>Delivery Date</th>
                <th>Added Date</th>
                <th>Approved By</th>
                <th>Approved Date</th>
                <th>Shipping Charges</th>
            </tr>
        </thead>
    </table>`;

        document.querySelector('#eventModal .modal-body').innerHTML = html;

        // Initialize DataTable
        new DataTable('#rqTable', {
            data: props.Items,
            destroy: true,
            ordering: false,
            columns: [
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row, meta) {
                        return meta.row + 1
                    }
                },
                { data: "RegisterQuotationCode", visible: false },
                { data: "RFQCode", render: safe },
                { data: "VendorName", render: safe },
                { data: "StatusName", render: safe },
                { data: "AddedBy", render: safe },
                { data: "DeliveryDate", render: safe },
                { data: "AddedDate", render: safe },
                { data: "ApprovedBy", render: safe },
                { data: "ApprovedDate", render: safe },
                {
                    data: "ShippingCharges",
                    render: function (data, type, row) {
                        return '₹ ' + safe(data);
                    }
                }
            ],
            columnDefs: [
                { className: "text-center", targets: "_all" }
            ],
            language: {
                lengthMenu: ""
            },
        });
    }

    // Render Purchase Order
    function renderPO(props) {
        if (!props) return;
        const modalTitle = document.querySelector('#eventModal .modal-title');
        modalTitle.textContent = "Purchase Order";

        // Prepare term conditions list
        const termList = (props.TermConditions && props.TermConditions.length > 0)
            ? `<ul>${props.TermConditions.map(tc => `<li>${safe(tc)}</li>`).join('')}</ul>`
            : "-";

        let html = `
        <div>
            <div class="container">
                <div class="row">
                    <div class="col-sm-6">
                        <strong>PO Code:</strong> ${safe(props.POCode)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Vendor Name:</strong> ${safe(props.VendorName)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Added By:</strong> ${safe(props.AddedBy)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Added Date:</strong> ${safe(props.AddedDate)}
                    </div>
                    <div class="col-sm-6">
                        <strong>${props.StatusName == 'Rejected' ? 'Rejected' : 'Approved'} By:</strong> ${safe(props.ApprovedBy)}
                    </div>
                    <div class="col-sm-6">
                        <strong>${props.StatusName == 'Rejected' ? 'Rejected' : 'Approved'} Date:</strong> ${safe(props.ApprovedDate)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Accountant Name:</strong> ${safe(props.AccountantName)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Status:</strong> ${safe(props.StatusName)}
                    </div>
                    <div class="col-sm-12">
                        <strong>Billing Address:</strong> ${safe(props.BillingAddress)}
                    </div>
                    <div class="col-sm-12 mb-3">
                        <strong>Term Conditions:</strong> ${termList}
                    </div>
                </div>
            </div>

            <hr/>

            <table id="poItemsTable" class="table table-striped table-bordered" style="width:100%">
                <thead class="table-dark text-uppercase">
                    <tr>
                    <th>Sr. No.</th>
                        <th class="d-none">POCode</th>
                        <th class="d-none">POItemCode</th>
                        <th class="d-none">RQItemCode</th>
                        <th class="d-none">ItemCode</th>
                        <th>Item Name</th>
                        <th>Cost Per Unit</th>
                        <th>Discount</th>
                        <th>Quantity</th>
                    </tr>
                </thead>
            </table><br/>
            <div class="text-end"><strong>Shipping Charges:</strong> ₹ ${safe(props.ShippingCharges)}</div>
            <div class="text-end"><strong>Total Amount: </strong> ₹ ${safe(props.TotalAmount)}</div>
        </div>
    `;

        document.querySelector('#eventModal .modal-body').innerHTML = html;

        // Initialize DataTable
        if (props.Items && props.Items.length > 0) {
            new DataTable('#poItemsTable', {
                data: props.Items,
                destroy: true,
                ordering: false,
                columns: [
                    {
                        data: null,
                        orderable: false,
                        searchable: false,
                        render: function (data, type, row, meta) {
                            return meta.row + 1
                        }
                    },
                    { data: "POCode", visible: false },
                    { data: "POItemCode", visible: false },
                    { data: "RQItemCode", visible: false },
                    { data: "ItemCode", visible: false },
                    { data: "ItemName", render: safe },
                    {
                        data: "CostPerUnit",
                        render: function (data, type, row) {
                            return '₹ ' + safe(data);
                        }
                    },
                    {
                        data: "Discount",
                        render: function (data, type, row) {
                            return safe(data) + ' %';
                        }
                    },
                    { data: "Quantity", render: safe }
                ],
                columnDefs: [
                    { className: "text-center", targets: "_all" }
                ],
                language: {
                    lengthMenu: ""
                },
            });
        }
    }

    // Render Goods Received Note
    function renderGRN(props) {
        if (!props) return;
        const modalTitle = document.querySelector('#eventModal .modal-title');
        modalTitle.textContent = "Goods Received Note";
        let html = `
        <div>
            <div class="container">
                <div class="row">
                    <div class="col-sm-6">
                        <strong>GRN Code:</strong> ${safe(props.GRNCode)}
                    </div>
                    <div class="col-sm-6">
                        <strong>PO Code:</strong> ${safe(props.POCode)}
                    </div>
                    <div class="col-sm-6">
                        <strong>GRN Date:</strong> ${safe(props.GRNDate)}
                    </div>
                    <div class="col-sm-6">
                        <strong>PO Date:</strong> ${safe(props.PODate)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Invoice Code:</strong> ${safe(props.InvoiceCode)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Invoice Date:</strong> ${safe(props.InvoiceDate)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Vendor:</strong> ${safe(props.VendorName)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Company Address:</strong> ${safe(props.CompanyAddress)}
                    </div>
                    <div class="col-sm-6 mb-3">
                        <strong>Billing Address:</strong> ${safe(props.BillingAddress)}
                    </div>
                </div>
            </div>
            
            <hr/>

            <table id="grnItemsTable" class="table table-striped table-bordered" style="width:100%">
                <thead class="table-dark text-uppercase">
                    <tr>
                    <th>Sr. No.</th>
                        <th class="d-none">GRNCode</th>
                        <th class="d-none">GRNItemcode</th>
                        <th class="d-none">ItemCode</th>
                        <th>Item Name</th>
                        <th>Quantity</th>
                        <th>Cost Per Unit</th>
                        <th>Discount</th>
                        <th>TaxRate</th>
                        <th>Final Amount</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table><br/>

            <div class="text-end"><strong>Shipping Charges:</strong> ₹ ${safe(props.ShippingCharges)}</div>
            <div class="text-end"><strong>Total Amount:</strong> ₹ ${safe(props.TotalAmount)}</div>
        </div>`;

        document.querySelector('#eventModal .modal-body').innerHTML = html;

        if (props.Items && props.Items.length > 0) {
            console.log(props.Items);
            console.log(props.Items[0]);
            new DataTable('#grnItemsTable', {
                data: props.Items,
                destroy: true,
                ordering: false,
                columns: [
                    {
                        data: null,
                        orderable: false,
                        searchable: false,
                        render: function (data, type, row, meta) {
                            return meta.row + 1
                        }
                    },
                    { data: "GRNCode", visible: false },
                    { data: "GRNItemCode", visible: false },
                    { data: "ItemCode", visible: false },
                    { data: "ItemName", render: safe },
                    { data: "Quantity", render: safe },
                    {
                        data: "CostPerUnit",
                        render: function (data, type, row) {
                            return '₹ ' + safe(data);
                        }
                    },
                    {
                        data: "Discount",
                        render: function (data, type, row) {
                            return safe(data) + ' %';
                        }
                    },
                    { data: "TaxRate", render: safe },
                    {
                        data: "FinalAmount",
                        render: function (data, type, row) {
                            return '₹ ' + safe(data);
                        }
                    },
                ],
                columnDefs: [
                    { className: "text-center", targets: "_all" }
                ],
                language: {
                    lengthMenu: ""
                },
            });
        }
    }

    // Render Goods Return
    function renderGoodsReturn(props) {
        if (!props) return;
        const modalTitle = document.querySelector('#eventModal .modal-title');
        modalTitle.textContent = "Goods Return";
        let html = `
        <div>
            <div class="container">
                <div class="row">
                    <div class="col-sm-6">
                        <strong>Goods Return Code:</strong> ${safe(props.GoodsReturnCode)}
                    </div>
                    <div class="col-sm-6">
                        <strong>GRN Code:</strong> ${safe(props.GRNCode)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Added By:</strong> ${safe(props.AddedBy)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Added Date:</strong> ${safe(props.AddedDate)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Transporter Name:</strong> ${safe(props.TransporterName)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Transport Contact No:</strong> ${safe(props.TransportContactNo)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Vehicle Type:</strong> ${safe(props.VehicleType)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Vehicle No:</strong> ${safe(props.VehicleNo)}
                    </div>
                    <div class="col-sm-6">
                        <strong>Status:</strong> ${safe(props.Status)}
                    </div>
                    <div class="col-sm-6 mb-3">
                        <strong>Reason:</strong> ${safe(props.Reason)}
                    </div>
                </div>
            </div>
        </div>
        <hr/>
        <table id="grItemsTable" class="table table-striped table-bordered" style="width:100%">
            <thead class="table-dark text-uppercase">
                <tr>
                    <th>Sr. No.</th>
                    <th class="d-none">GR Item Code</th>
                    <th class="d-none">Item Code</th>
                    <th>Item Name</th>
                    <th>Reason</th>
                </tr>
            </thead>
        </table>
    `;

        document.querySelector('#eventModal .modal-body').innerHTML = html;

        if (props.Items && props.Items.length > 0) {
            new DataTable('#grItemsTable', {
                data: props.Items,
                destroy: true,
                ordering: false,
                columns: [
                    {
                        data: null,
                        orderable: false,
                        searchable: false,
                        render: function (data, type, row, meta) {
                            return meta.row + 1
                        }
                    },
                    { data: "GRItemCode", visible: false },
                    { data: "ItemCode", visible: false },
                    { data: "ItemName", render: safe },
                    { data: "Reason", render: safe }
                ],
                columnDefs: [
                    { className: "text-center", targets: "_all" }
                ],
                language: {
                    lengthMenu: ""
                },
            });
        }
    }

    // Render Quality Check

    function renderQC(props) {
        if (!props) return;
        const modalTitle = document.querySelector('#eventModal .modal-title');
        modalTitle.textContent = "Quality Check";
        let html = `
        <table id="qcItemsTable" class="table table-striped table-bordered" style="width:100%">
            <thead class="table-dark text-uppercase">
                <tr>
                    <th>Sr. No.</th>
                    <th class="d-none">QC Code</th>
                    <th class="d-none">GRN Item Code</th>
                    <th class="d-none">Item Code</th>
                    <th>Item Name</th>
                    <th>Quantity</th>
                    <th>Inspection Frequency</th>
                    <th>Sample Checked</th>
                    <th>Status</th>
                    <th>Sample Failed</th>
                    <th>QC Added By</th>
                    <th>QC Added Date</th>
                    <th>QC Failed By</th>
                    <th>QC Failed Date</th>
                    <th>Reason</th>
                </tr>
            </thead>
        </table>`;

        document.querySelector('#eventModal .modal-body').innerHTML = html;

        if (props.Items && props.Items.length > 0) {
            console.log(props.Items);
            let table = new DataTable('#qcItemsTable', {
                data: props.Items,
                destroy: true,
                ordering: false,
                columns: [
                    {
                        data: null,
                        orderable: false,
                        searchable: false,
                        render: function (data, type, row, meta) {
                            return meta.row + 1
                        }
                    },
                    { data: "QualityCheckCode", visible: false },
                    { data: "GRNItemsCode", visible: false },
                    { data: "ItemCode", visible: false },
                    { data: "ItemName", render: safe },
                    { data: "Quantity", render: safe },
                    { data: "InspectionFrequency", render: safe },
                    { data: "SampleQualityChecked", render: safe },
                    { data: "StatusName", render: safe },
                    { data: "SampleTestFailed", render: safe },
                    { data: "QCAddedBy", render: safe },
                    { data: "QCAddedDate", render: safe },
                    { data: "QCFailedAddedBy", render: safe },
                    { data: "QCFailedDate", render: safe },
                    { data: "Reason", render: safe }
                ],
                columnDefs: [
                    { className: "text-center", targets: "_all" }
                ],
                language: {
                    lengthMenu: ""
                },
            });

            if (props.Items[0].StatusName === "Confirmed") {
                table.column(11).visible(false);
                table.column(12).visible(false);
                table.column(13).visible(false);
            }
        }
    }
});