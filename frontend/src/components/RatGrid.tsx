import { useCallback, useContext, useEffect, useState } from 'react';
import { useLocation, useNavigate } from "react-router-dom";
import axios from 'axios';
import { DataGrid, GridColDef, GridColumnVisibilityModel } from '@mui/x-data-grid';
import Button from '@mui/material/Button';
import OpenInBrowserIcon from '@mui/icons-material/OpenInBrowser';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import RatIcon from './RatIcon';
import RatMultiSelect from './RatMultiSelect';
import RatLocales from '../contexts/RatLocales';
import RatUser from '../contexts/RatUser';
import RatAppContext from '../contexts/RatAppContext';
import { IconsByString } from '../fonts/IconsByString';
import { GridColumn, SelectOptions } from './types';

function lowerFirstLetter(str: string) {
    return str.charAt(0).toLowerCase() + str.slice(1);
}

function RatGrid(props: GridProps) {
    const [paginationModel, setPaginationModel] = useState({ pageSize: 15, page: 0 });
    const [rawColumns, setRawColumns] = useState<Array<GridColumn>>([]);
    const [columns, setColumns] = useState<Array<GridColDef>>([]);
    const [gridData, setGridData] = useState([]);
    const [hiddenColumns, setHiddenColumns] = useState<GridColumnVisibilityModel>({});
    const locales = useContext(RatLocales);
    const user = useContext(RatUser);
    const { languageId } = useContext(RatAppContext);
    const navigate = useNavigate();
    const location = useLocation();

    // Locale for date formatting: culture of the selected language, fallback to en-us.
    const dateCulture = user.data.languages?.find(
        (language) => language.id === languageId
    )?.languageCulture || 'en-us';

    const getGridData = useCallback(function () {
        axios.post("/entity/getalltotable", { entityName: props.entityName })
            .then(function (response) {
                setRawColumns(response.data.columns);
                setGridData(response.data.data);
            })
            .catch(function (error) {
                console.error("Failed to load grid data", error);
            });
    }, [props.entityName]);

    const entityRedirect = useCallback(function (id: number) {
        let url = location.pathname.replace(/\/$/, "");
        navigate(url + "/" + id);
    }, [location.pathname, navigate]);

    const deleteEntry = useCallback(function (id: number) {
        axios.post("/entity/deleteentity", { entityName: props.entityName, id: id })
            .then(function () {
                getGridData();
            })
            .catch(function (error) {
                console.error("Failed to delete entity", error);
            });
    }, [props.entityName, getGridData]);

    // Rebuild columns whenever raw data or locales change so headers/labels stay localized.
    const buildColumns = useCallback(function () {
        let columnsData: Array<GridColDef> = [];
        const optionsData: { [field: string]: SelectOptions } = {};

        rawColumns.forEach(function (column: GridColumn) {
            let columnObject: GridColDef = {
                field: lowerFirstLetter(column.name),
                headerName: column.entryType !== "EnumIcon" ? locales[column.name] : ""
            };

            switch (column.entryType) {
                case "String": {
                    columnObject["flex"] = 1;
                    break;
                }
                case "Boolean": {
                    columnObject["renderCell"] = ({ value }) => (
                        <>
                            {value ? <RatIcon name="task_alt" />
                                : <RatIcon name="radio_button_unchecked" />}
                        </>
                    );
                    columnObject["width"] = 150;
                    break;
                }
                case "DateTime": {
                    columnObject["valueGetter"] = (value: string) => {
                        if (!value) {
                            return "";
                        }
                        const date = new Date(value);
                        return isNaN(date.getTime()) ? "" : date.toLocaleString(dateCulture);
                    };
                    columnObject["flex"] = 1;
                    break;
                }
                case "Enum": {
                    optionsData[columnObject.field] = column.selectOptions;
                    columnObject["valueGetter"] = (value: number) => {
                        return optionsData[columnObject.field]?.[value];
                    };
                    columnObject["flex"] = 1;
                    break;
                }
                case "EnumIcon": {
                    optionsData[columnObject.field] = column.selectOptions;
                    columnObject["renderCell"] = ({ value }) => {
                        const option = optionsData[columnObject.field]?.[value];
                        return option ? <RatIcon class={lowerFirstLetter(option)} name={IconsByString[option]} /> : null;
                    };
                    columnObject["width"] = 50;
                    break;
                }
                case "MappedMultiSelect": {
                    columnObject["renderCell"] = ({ row }) => (
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
                    columnObject["renderCell"] = ({ row }) => (
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
                    columnObject["renderCell"] = ({ row }) => (
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
                    columnObject["renderCell"] = ({ row }) => (
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
    }, [rawColumns, locales, dateCulture, entityRedirect, deleteEntry]);

    // Default visibility depends only on the schema; merge so user toggles win.
    useEffect(() => {
        const defaults: GridColumnVisibilityModel = {};

        rawColumns.forEach(function (column: GridColumn) {
            const field = lowerFirstLetter(column.name);
            if (field === "id" || column.hidden) {
                defaults[field] = false;
            }
        });

        setHiddenColumns(function (prev) {
            return { ...defaults, ...prev };
        });
    }, [rawColumns])

    useEffect(() => {
        getGridData();
    }, [getGridData])

    useEffect(() => {
        buildColumns();
    }, [buildColumns])

    return (
        <DataGrid
            sx={{ width: '100%', height: 750, margin: '10px 0 0' }}
            rows={gridData}
            columns={columns}
            paginationModel={paginationModel}
            onPaginationModelChange={setPaginationModel}
            pageSizeOptions={[15, 25, 50]}
            columnVisibilityModel={hiddenColumns}
            onColumnVisibilityModelChange={setHiddenColumns}
        />
    );
}

export default RatGrid;

type GridProps = {
    entityName: string;
}
