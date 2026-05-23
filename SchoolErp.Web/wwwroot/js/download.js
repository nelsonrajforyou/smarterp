window.downloadFileFromStream = async (fileName, contentStreamReference) => {
  const arrayBuffer = await contentStreamReference.arrayBuffer();
  const blob = new Blob([arrayBuffer]);
  const url = URL.createObjectURL(blob);
  const anchorElement = document.createElement('a');
  anchorElement.href = url;
  anchorElement.download = fileName ?? '';
  anchorElement.click();
  anchorElement.remove();
  URL.revokeObjectURL(url);
}

window.exportTableToExcel = (tableSelector, filename) => {
  const table = document.querySelector(tableSelector);
  if (!table) return;

  let csvContent = "";
  const rows = table.querySelectorAll("tr");
  
  rows.forEach(row => {
    const cols = row.querySelectorAll("th, td");
    const rowData = [];
    cols.forEach(col => {
      let text = col.innerText.trim().replace(/"/g, '""');
      if (text.includes(",") || text.includes("\n") || text.includes('"')) {
        text = `"${text}"`;
      }
      rowData.push(text);
    });
    csvContent += rowData.join(",") + "\n";
  });

  const blob = new Blob(["\ufeff" + csvContent], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = filename || "Report.csv";
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(url);
};
