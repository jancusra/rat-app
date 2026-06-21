import { useContext, useEffect, useState } from 'react';
import { useNavigate } from "react-router-dom";
import axios from 'axios';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import Button from '@mui/material/Button';
import OpenInBrowserIcon from '@mui/icons-material/OpenInBrowser';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import RatIcon from './RatIcon';
import RatMultiSelect from '../components/RatMultiSelect';
import RatLocales from '../contexts/RatLocales';
import { IconsByString } from '../fonts/IconsByString';
import { SelectOptions } from './types';

function RatGrid(props: GridProps) {
  const [paginationModel, setPaginationModel] = useState({ pageSize: 15, page: 0 });
  const [columns, setColumns] = useState<Array<GridColDef>>([]);
  const [gridData, setGridData] = useState([]);
  const locales = useContext(RatLocales);
  const navigate = useNavigate();
  const hiddenColumns = {}, optionsData = {};

  function lowerFirstLetter(str: string) {
    return str.charAt(0).toLowerCase() + str.slice(1);
  }

  function entityRedirect(id: number) {
    let url = location.pathname.replace(/\/$/, "");
    navigate(url + "/" + id);
  }

  function deleteEntry(id: number) {
    axios.post("/entity/deleteentity", { entityName: props.entityName, id: id })
      .then(function () {
        getGridData();
      });
  }

  function getGridData() {
    axios.post("/entity/getalltotable", { entityName: props.entityName })
      .then(function (response) {
        let columnsData: Array<GridColDef> = [];
        
        response.data.columns.forEach(function (column: GridColumn) {
          let columnObject = {
            field: lowerFirstLetter(column.name),
            headerName: column.entryType != "EnumIcon" ? locales[column.name] : ""
          };

          if (columnObject.field == "id" || column.hidden)
          {
            hiddenColumns[columnObject.field] = false;
          }

          switch(column.entryType) {
            case "String": {
              columnObject["flex"] = 1;
              break;
            }
            case "Boolean": {
              columnObject["renderCell"] = ({value}) => (
                <>
                { value ? <RatIcon name="task_alt" />
                    : <RatIcon name="radio_button_unchecked" /> }
                </>
              );
              columnObject["width"] = 150;
              break;
            }
            case "DateTime": {
              columnObject["valueGetter"] = (value: string) => {
                const date = new Date(value);
                return date.toLocaleString('en-us');
              };
              columnObject["flex"] = 1;
              break;
            }
            case "Enum": { 
              optionsData[columnObject.field] = column.selectOptions;
              columnObject["valueGetter"] = (value: string) => {
                return optionsData[columnObject.field][value];
              };
              columnObject["flex"] = 1;
              break; 
            }
            case "EnumIcon": {
              optionsData[columnObject.field] = column.selectOptions;
              columnObject["renderCell"] = ({value}) => (
                <RatIcon class={lowerFirstLetter(optionsData[columnObject.field][value])} 
                  name={IconsByString[optionsData[columnObject.field][value]]} />
              );
              columnObject["width"] = 50;
              break;
            }
            case "MappedMultiSelect": {
              columnObject["renderCell"] = ({row}) => (
                <RatMultiSelect
                  name={columnObject.field}
                  readOnly
                  stringValues
                  value={row[columnObject.field]}
                  selectData={column.selectOptions} />
              );
              columnObject["width"] = 600;
              break;
            }
            case "ShowDetailButton": {
              columnObject["renderCell"] = ({row}) => (
                <Button variant="contained"
                  color="primary"
                  startIcon={<OpenInBrowserIcon />}
                  onClick={() => entityRedirect(row.id)}>
                  {locales.Detail}
                </Button>
              );
              columnObject["width"] = 150;
              break;
            }
            case "EditInViewButton": {
              columnObject["renderCell"] = ({row}) => (
                <Button variant="contained"
                  color="secondary"
                  startIcon={<EditIcon />}
                  disabled={row.hasOwnProperty("isSystemEntry") ? row.isSystemEntry : false}
                  onClick={() => entityRedirect(row.id)}>
                  {locales.Edit}
                </Button>
              );
              columnObject["width"] = 150;
              break;
            }
            case "DeleteButton": {
              columnObject["renderCell"] = ({row}) => (
                <Button variant="contained"
                  color="error"
                  startIcon={<DeleteIcon />}
                  disabled={row.hasOwnProperty("isSystemEntry") ? row.isSystemEntry : false}
                  onClick={() => deleteEntry(row.id)}>
                  {locales.Delete}
                </Button>
              );
              columnObject["width"] = 150;
              break;
            }
            default: {
              break; 
            }
          }

          if (column.width > 0) {
            columnObject["width"] = column.width;
          }

          if (column.flex > 0) {
            columnObject["flex"] = column.flex;
          }

          columnsData.push(columnObject);
        });

        setColumns(columnsData);
        setGridData(response.data.data);
    });
  }

  useEffect(() => {
      getGridData();
  }, [])

  return (
      <DataGrid
          sx={{ width: '100%', height: 750, margin: '10px 0 0' }}
          rows={gridData}
          columns={columns}
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          pageSizeOptions={[15, 25, 50]}
          initialState= {{
            columns: {
              columnVisibilityModel: hiddenColumns
            }
          }}
      />
  );
}

export default RatGrid;

type GridProps = {
  entityName: string;
}

type GridColumn = {
  entryType: string;
  excluded: boolean;
  hidden: boolean;
  name: string;
  order: number;
  selectOptions: SelectOptions;
  flex: number;
  width: number;
}