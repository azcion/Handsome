﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Handsome.Source;

namespace Handsome.Prefabs {

	internal sealed partial class ControlEntry {

		private static readonly Color Green = Color.FromArgb(74, 202, 168);

		private readonly int _id;
		private readonly FormClient _form;

		public ControlEntry (FormClient form, Entry entry, int id) {
			InitializeComponent();

			_id = id;
			_form = form;

			AssembleLabels(entry);
			AssembleDataGrid(entry.Data);

			Dock = DockStyle.Top;
			AutoSize = true;
			ResizeGrid(null, null);
		}

		private static void SetStyle (ref DataGridViewCell cell, bool restore = false) {
			if (restore) {
				if (cell.RowIndex % 2 == 0) {
					cell.Style.BackColor = cell.DataGridView.DefaultCellStyle.BackColor;
					cell.Style.ForeColor = cell.DataGridView.DefaultCellStyle.ForeColor;
				} else {
					cell.Style.BackColor = cell.DataGridView.AlternatingRowsDefaultCellStyle.BackColor;
					cell.Style.ForeColor = cell.DataGridView.AlternatingRowsDefaultCellStyle.ForeColor;
				}

				return;
			}

			cell.Style.BackColor = Color.IndianRed;
			cell.Style.ForeColor = Color.White;
		}

		private void AssembleLabels (Entry entry) {
			_checkOutLabel.Text = entry.IsCheckout ? "Odjava" : "";
			_dateLabel.Text = entry.Date;
			_valueLabel.Text = Row.Format(entry.Value);

			_dateLabel.TextChanged += DateChanged;
		}

		private void AssembleDataGrid (IEnumerable<Row> data) {
			_dataGrid.ColumnCount = 4;
			_dataGrid.Columns[0].Name = "Količina";
			_dataGrid.Columns[1].Name = "Izdelek";
			_dataGrid.Columns[2].Name = "Cena/kos";
			_dataGrid.Columns[3].Name = "Vrednost";

			foreach (Row row in data) {
				int index = _dataGrid.Rows.Add(row.ToObjectArray());
				_dataGrid.Rows[index].Cells[3].ReadOnly = true;
			}

			_dataGrid.BackgroundColor = Green;

			ResizeGrid(null, null);
			_dataGrid.UserAddedRow += ResizeGrid;
			_dataGrid.CellValueChanged += UpdateGrid;
			_dataGrid.RowsRemoved += UpdateGrid;
		}

		#region Event handlers

		private void ResizeGrid (object sender, EventArgs e) {
			const int gapSize = 10;
			int h = _dataGrid.ColumnHeadersHeight + _checkOutLabel.Height;
			h += _dataGrid.RowCount * _dataGrid.RowTemplate.Height;
			_entryPanel.Height = h + gapSize;
		}

		private void DateChanged (object sender, EventArgs e) {
			_form.UpdateDate(_dateLabel.Text, _id);
		}

		private void UpdateGrid (object sender, EventArgs e) {
			DataGridView dataGrid = sender as DataGridView;
			bool didFail = false;
			List<Row> data = new List<Row>();

			if (dataGrid == null) {
				return;
			}

			for (var i = 0; i < dataGrid.RowCount - 1; i++) {
				DataGridViewRow row = dataGrid.Rows[i];
				DataGridViewCell quantityCell = row.Cells[0];
				DataGridViewCell nameCell = row.Cells[1];
				DataGridViewCell priceCell = row.Cells[2];

				if (int.TryParse(quantityCell.Value?.ToString(), out int quantity) == false) {
					SetStyle(ref quantityCell);
					didFail = true;
				} else {
					SetStyle(ref quantityCell, true);
				}

				if (float.TryParse(priceCell.Value?.ToString().Replace(',', '.'), out float price) == false) {
					SetStyle(ref priceCell);
					didFail = true;
				} else {
					SetStyle(ref priceCell, true);
				}

				row.Cells[3].Value = Row.Format(quantity * price);

				if (nameCell.Value == null) {
					didFail = true;
					SetStyle(ref nameCell);
				} else {
					SetStyle(ref nameCell, true);
				}

				if (didFail == false) {
					data.Add(new Row(quantity, nameCell.Value.ToString(), price));
				}
			}

			_form.UpdateData(data, _dateLabel.Text, didFail, _id);
		}

		#endregion

	}

}