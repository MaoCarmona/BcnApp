import os, fnmatch                        # os para listar los archivos 
import json
from requests.auth import HTTPBasicAuth 
import shutil                             # shutil para mover los archivos de una carpeta a otra
from tkinter import messagebox            # Mensaje de la ventana
import xml.etree.ElementTree as ET        # libreria para leer los archivos xml  
from datetime import datetime, timedelta  # libreria para el manejo de fecha y hora  
import time
import tkinter as tk                      # Libreria para el maenjo de ventana GUI 
from tkinter import font, ttk             # Libreria para el maenjo de ventana GUI 
import pandas as pd                       # Libreria para el maenjo de archivos Excel  
import jwt                                # Libreria para el toquen 
import pymssql                            # Libreria para conectarse a la Base de Datos SQL Server
import requests 
import locale                            # Configuracion Regional 

def EnviarInfoWSARES(IPayLoad, Iendpoint, IUsr, IPwd, ITransaccionOrigen):
    headers = {'Content-Type': 'application/json; charset=utf-8'}
    vtimestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")   
    # print('Usuario: ', IUsr)    
    # print('pwd: ', IPwd)    
    # print('endpoint: ', Iendpoint) 
    # print('methodo: ', ItxXML) 
    try:
        # payload = f"""<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:int="http://COMMON/Interfaces"><soapenv:Header/><soapenv:Body><int:MT_RespuestaLegado>{ItxXML}</int:MT_RespuestaLegado></soapenv:Body></soapenv:Envelope>"""            
        response = requests.post(Iendpoint, headers=headers, data = json.dumps(IPayLoad), auth = HTTPBasicAuth(IUsr, IPwd))
        vCode = response.status_code
        # print("Envio ", IPayLoad)
        if response.status_code == 200:
            vMsg = vtimestamp + " INFO: Envio exitoso del Registro " + ITransaccionOrigen + " (estado: 200)"
            # print(response.text, " ", response.status_code)
            time.sleep(0.05) # demora de 0.5 segundos
        else:
            vMsg = vtimestamp + " INFO: Existe un error: " + ITransaccionOrigen + " (Estado " +str(response.status_code) + " )"
    except requests.ConnectionError:
        vMsg = vtimestamp + " ERROR: Estado: 400 - No se pudo conectar con el servidor. Asegurese de que el servidor este en funcionamiento.\n"
        vCode = 400
    except requests.Timeout:
        vMsg = vtimestamp + " ERROR: Estado: 500 - Se agotó el tiempo de espera de la solicitud. Verifique su conexión a Internet o vuelva a intentarlo más tarde.\n"
        vCode = 500
    except Exception as e:
        vMsg = vtimestamp + " ERROR: Se produjo el siguiente : " + str(e) + "\n"
        vCode = 0
    return vCode, vMsg

def EnvioARES(ItpInfo, IarrParametros, IUsrAuditoria):
    vdtFormato = "%Y-%m-%d %H:%M:%S"
    vFuente = "GRB"   
    vFechaAuxI = txtFechaIni.get() + " 00:00:00" 
    vFechaAuxF = txtFechaFin.get() + " 23:59:59" 
    # vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea enviar la información: " + vIdOpcion[3:].upper() + " al Webservice de ARES ? \n\npaara el perido Desde: "+ str(vFechaAuxI) + " Hasta: "+ str(vFechaAuxF), title=vFuente + " .::AppBCN")                    
    vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea enviar la información: " + ItpInfo.upper() + " al Webservice de ARES ? \n\n", title=vFuente + " .::AppBCN")                    
    if vRespuesta == True:        
        vDatosDB = varrParametros["InfoDBBCN"]    
        vUsr = varrParametros["InfoURLARES"]["idUsr"]
        vPwd = varrParametros["InfoURLARES"]["pwUsr"]
        vEndPoint = varrParametros["InfoURLARES"]["txURL"]
        
        if ItpInfo == 'COSTOS':        
            vTagQuery = "WSCOSTOS"        
            vSql = varrParametros["xQuerys"].find("qry" + vTagQuery)             
            vSqlAux = vSql.text.strip() 
            vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(vFechaAuxI))
            vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(vFechaAuxF))
            vSqlAux = vSqlAux.replace("[Filtro]", "AND c.txProcesamiento NOT LIKE 'Documento:%'") 
            vtxJSON = varrParametros["txMovCostos"]
            vTransaccionOrigen = "Movimiento de Costo"
            vEndPoint = vEndPoint + varrParametros["InfoURLARES"]["txMetodoCosto"]
        elif ItpInfo == 'INVENTARIOS':
            vTagQuery = "WSINVLOGISTICO"        
            vSql = IarrParametros["xQuerys"].find("qry" + vTagQuery)             
            vSqlAux = vSql.text.strip()
            vFechaAux = datetime.strptime(vFechaAuxF, vdtFormato) + timedelta(seconds =-59)        
            vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(vFechaAux))
            # vtxJSON = IarrParametros["txMovLogistico"]
            vtxJSON = ""
            vTransaccionOrigen = "Inventario Logistico"
            vEndPoint = vEndPoint + varrParametros["InfoURLARES"]["txMetodoInventario"]
        elif ItpInfo == 'MOVIMIENTOS':        
            vTagQuery = "WSMOVLOGISTICO"    
            vTransaccionOrigen = "Movimiento Logistico"
            vEndPoint = vEndPoint + varrParametros["InfoURLARES"]["txMetodoMovimiento"]
            vSql = IarrParametros["xQuerys"].find("qry" + vTagQuery)
            vSqlAux = vSql.text.strip() 
            vSqlAux = vSqlAux.replace("[dtConsultaIni]", str(vFechaAuxI))
            vSqlAux = vSqlAux.replace("[dtConsultaFin]", str(vFechaAuxF))
            vSqlAux = vSqlAux.replace("[Filtro]", "AND ml.txProcesamiento NOT LIKE 'Documento:%'")    
            vtxJSON = IarrParametros["txMovLogistico"]
        
        # "https://aeuecpaaorab01d.red.ecopetrol.com.co/IntegracionBICAORA/Api/CargarCostosBIC"    
        # print(vUsr, " ", vPwd, " ", vEndPoint)    
        vMsg, vDatosRS = oConectarDB(vDatosDB, vSqlAux)
        # vDatosRS = []
        # print(vSqlAux + "\n")    
        # print(len(vDatosRS))
            
        if len(vDatosRS):
            IUsrAuditoria = "AdminBCN"
            
            vAux = " para el perido: " + str(vFechaAuxI) + " a " + str(vFechaAuxF)
            if ItpInfo == 'INVENTARIOS':
                vAux = " para fecha : " + str(vFechaAux)
            messagebox.showinfo(message="Se va a enviar " + str(len(vDatosRS)) + " registros de " + ItpInfo + vAux, title="AppBCN")
            for I in range(len(vDatosRS)):
                vtimestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                vtxJSONAux = vtxJSON
                if ItpInfo == 'INVENTARIOS':    
                    # vtxJSONAux = vtxJSONAux.replace('[idMsgCosto]', str(vDatosRS[I]["idMsgCostos"]))
                    vPayLoad = {"dtContabilizacion": str(vDatosRS[I]["dtContabilizacion"]),
                                "idRecurso": str(vDatosRS[I]["nmRecAlmacen"]),
                                "idProducto": str(vDatosRS[I]["nmRecProducto"]),
                                "idCELO": str(vDatosRS[I]["nbCenLog"]),
                                "idALMACEN": str(vDatosRS[I]["nbAlmLog"]),
                                "idMaterial": str(vDatosRS[I]["nbMaterial"]),
                                "vlContable": str(vDatosRS[I]["vlContable"]),
                                "idUMContable": str(vDatosRS[I]["idUM"]),
                                "idUsrAuditoria": str(IUsrAuditoria),
                                "dtUsrAuditoria": vtimestamp
                            }                     
                    vTransaccionOrigenAux = vTransaccionOrigen + " " + str(vDatosRS[I]["nmRecAlmacen"]) + " " + str(vDatosRS[I]["dtContabilizacion"]) 
                elif ItpInfo == 'COSTOS':                
                    vdtContable = vDatosRS[I]["dtContabilizacion"].replace("-", "")
                    # vtxJSONAux = vtxJSONAux.replace('[idMensaje]', '')
                    vtxJSONAux = vtxJSONAux.replace('[idMsgCosto]', str(vDatosRS[I]["idMsgCostos"]))            
                    vtxJSONAux = vtxJSONAux.replace('[dtContabilizacion]', str(vdtContable))
                    vtxJSONAux = vtxJSONAux.replace('[idRegistro]', str('AdminBCN'))                
                    vtxJSONAux = vtxJSONAux.replace('[vlContabilizado]', str(vDatosRS[I]["vlContable"]))
                    vtxJSONAux = vtxJSONAux.replace('[ObjVolTotal]', str(vDatosRS[I]["ObjVolTotal"]))
                    vtxJSONAux = vtxJSONAux.replace('[ObjColector]', str(vDatosRS[I]["ObjColector"]))
                    vtxJSONAux = vtxJSONAux.replace('[ObjPlantaPool]', str(vDatosRS[I]["ObjPlantaPool"]))        
                    vtxJSONAux = vtxJSONAux.replace('[ObjEstadistico]', str(vDatosRS[I]["ObjEstadistico"]))                            
                    vtxJSONAux = vtxJSONAux.replace('\\"', '\"')
                    vtxJSONAux = vtxJSONAux.replace('"{}"', '{}')
                    vtxJSONAux = vtxJSONAux.replace('"{', '{')
                    vtxJSONAux = vtxJSONAux.replace('}"', '}')
                    vPayLoad = {"idMessage": str(vDatosRS[I]["idMovCostoReg"]),
                                "tpObjCostos": str(vDatosRS[I]["tpObjCosto"]),
                                "txMovimiento": str(vDatosRS[I]["tpObjCosto"]) + ': ' + str(vDatosRS[I]["nbObjCosto"]) + ' - '+str(vDatosRS[I]["ObjEstadistico"]),
                                "dtContabilizacion": str(vDatosRS[I]["dtContabilizacion"]),	
                                "idObjCosto": str(vDatosRS[I]["nbObjCosto"]),
                                "idValEstadistico": str(vDatosRS[I]["ObjEstadistico"]),
                                "nmProducto": str(vDatosRS[I]["nmProducto"]),
                                "idUM": str(vDatosRS[I]["idUM"]),
                                "vlContabilizado": str(vDatosRS[I]["vlContable"]),
                                "jsMovimiento": vtxJSONAux,
                                "txEstadoEnvio": str(vDatosRS[I]["txProcesamiento"]),
                                "idUsrAuditoria": str(IUsrAuditoria),                                
                                "dtUsrAuditoria": vtimestamp
                                }                            
                    vTransaccionOrigenAux = vTransaccionOrigen + " " + str(vdtContable) + " "+ str(vDatosRS[I]["idMovCostoReg"]) + ' - '+str(vDatosRS[I]["ObjEstadistico"]) 
                elif ItpInfo == 'MOVIMIENTOS':
                    vtxJSONAux = vtxJSONAux.replace('[idMsg]', str(vDatosRS[I]["idRegMovLogistico"]))
                    vtxJSONAux = vtxJSONAux.replace('[idMsgMovimiento]', "SM-ARES-" + str(vDatosRS[I]["idRegMovLogistico"]))
                    vtxJSONAux = vtxJSONAux.replace('[dtCargue]', str(vDatosRS[I]["dtContabilizacion"]))
                    
                    vtxJSONAux = vtxJSONAux.replace('[dtMovIni]', str(vDatosRS[I]["dtMovimientoIni"]))
                    vtxJSONAux = vtxJSONAux.replace('[dtMovFin]', str(vDatosRS[I]["dtMovimientoFin"]))            

                    vtxJSONAux = vtxJSONAux.replace('[nbClsMov]', str(vDatosRS[I]["nbMovimientoCls"]))
                    vtxJSONAux = vtxJSONAux.replace('[nbGMCODE]', str(vDatosRS[I]["nbGM"]))
                    vtxJSONAux = vtxJSONAux.replace('[tpInventario]', str(vDatosRS[I]["tpInventario"]))  
                    
                    vtxJSONAux = vtxJSONAux.replace('[numPedido]', str(vDatosRS[I]["numPedido"]))
                    vtxJSONAux = vtxJSONAux.replace('[posPedido]', str(vDatosRS[I]["posPedido"]))            
                    vtxJSONAux = vtxJSONAux.replace('[nbCenLogOrigen]', str(vDatosRS[I]["nbCenLogOrigen"]))
                    vtxJSONAux = vtxJSONAux.replace('[nbAlmLogOrigen]', str(vDatosRS[I]["nbAlmLogOrigen"]))
                    vtxJSONAux = vtxJSONAux.replace('[nbProdOrigen]', str(vDatosRS[I]["nbProdLogOrigen"]))            
                    vtxJSONAux = vtxJSONAux.replace('[nbCenLogDestino]', str(vDatosRS[I]["nbCenLogDestino"]))
                    vtxJSONAux = vtxJSONAux.replace('[nbAlmLogDestino]', str(vDatosRS[I]["nbAlmLogDestino"]))
                    vtxJSONAux = vtxJSONAux.replace('[nbProdDestino]', str(vDatosRS[I]["nbProdLogDestino"]))            
                    # Validacion para la unidad del pedido
                    if vDatosRS[I]["numPedido"] == '':                    
                        vtxJSONAux = vtxJSONAux.replace('[CantNS]', str(vDatosRS[I]["vlContable"]))
                        vtxJSONAux = vtxJSONAux.replace('[cantNSUM]', str(vDatosRS[I]["idUM"]))                    
                    else:
                        vtxJSONAux = vtxJSONAux.replace('[CantNS]', str(vDatosRS[I]["vlQCI"]))
                        vtxJSONAux = vtxJSONAux.replace('[cantNSUM]', str(vDatosRS[I]["idUMQCI"]))                    	
                    vtxJSONAux = vtxJSONAux.replace('[nbCentroCosto]', str(vDatosRS[I]["nbCentroCosto"]))            
                    vtxJSONAux = vtxJSONAux.replace('[txAtrCalidad]', str(vDatosRS[I]["txAtrCalidad"]))
                    vtxJSONAux = vtxJSONAux.replace('[txQCI]', str(vDatosRS[I]["txQCI"]))
    
                    vtxJSONAux = vtxJSONAux.replace('[nmPropietario]', str(vDatosRS[I]["idPropiedad"]))
                    vtxJSONAux = vtxJSONAux.replace('[idUsuario]', str(IUsrAuditoria))
                    
                    # vtxJSONAux = vtxJSONAux.replace('[idMsgCosto]', str(vDatosRS[I]["dtUsrAuditoria"]))

                    vtxJSONAux = vtxJSONAux.replace('\\"', '\"')
                    vtxJSONAux = vtxJSONAux.replace('"{}"', '{}')
                    vtxJSONAux = vtxJSONAux.replace('"{', '{')
                    vtxJSONAux = vtxJSONAux.replace('}"', '}')
                    # "IDMessageARES": "1000099",                
                    vPayLoad = {"IDMessage": str(vDatosRS[I]["idRegMovLogistico"]),                        
                                "dtContabilizacion": str(vDatosRS[I]["dtContabilizacion"]),
                                "idMovimiento": str(vDatosRS[I]["idMovimientoReg"]),   
                                                    
                                "dtMovimientoIni": str(vDatosRS[I]["dtMovimientoIni"]),
                                "dtMovimientoFin": str(vDatosRS[I]["dtMovimientoFin"]),                        
                                "tpMovimiento": str(vDatosRS[I]["tpMovimiento"]),                        
                                
                                "clsMovimiento": str(vDatosRS[I]["nbMovimientoCls"]),                            
                                "TransactionCodeSAP": str(vDatosRS[I]["nbGM"]),
                                "StockTypeSAP": str(vDatosRS[I]["tpInventario"]),                                    
                                
                                "NumPedido": str(vDatosRS[I]["numPedido"]),
                                "PosPedido": str(vDatosRS[I]["posPedido"]),          
                                            
                                "idRecOrigen": str(vDatosRS[I]["nmRecOrigen"]),
                                "idProdOrigen": str(vDatosRS[I]["nmProdOrigen"]),
                                "idRecDestino": str(vDatosRS[I]["nmRecDestino"]),
                                "idProdDestino": str(vDatosRS[I]["nmProdDestino"]),
                                
                                "idSRCCELO": str(vDatosRS[I]["nbCenLogOrigen"]),
                                "idSRCALMACEN": str(vDatosRS[I]["nbAlmLogOrigen"]),
                                "idSRCMaterial": str(vDatosRS[I]["nbProdLogOrigen"]),
                                                            
                                "idDSTCCELO": str(vDatosRS[I]["nbCenLogDestino"]),
                                "idDSTALMACEN": str(vDatosRS[I]["nbAlmLogDestino"]),
                                "idDSTMaterial": str(vDatosRS[I]["nbProdLogDestino"]),    
                                
                                "vlContable": str(vDatosRS[I]["vlContable"]),
                                "idUMContable": str(vDatosRS[I]["idUM"]),
                                "idCentroCosto": str(vDatosRS[I]["nbCentroCosto"]),              
                                "txEstadoEnvio": str(vDatosRS[I]["txProcesamiento"]),                             
                                "vlAtrCalidad": str(vDatosRS[I]["vlAtrCalidad"]),
                                "idUMAtrCalidad": str(vDatosRS[I]["idUMAtrCalidad"]),                                                    
                                "vlCantidaadQCI": str(vDatosRS[I]["vlQCI"]),
                                "idUMCantidadQCI": str(vDatosRS[I]["idUMQCI"]),
                                "txCantidadQCI": str(vDatosRS[I]["upQCI"]),                            
                                "IdPropiedad": str(vDatosRS[I]["idPropiedad"]),                        
                                "jsMovimiento": vtxJSONAux,
                                "idUsrAuditoria": str(IUsrAuditoria),
                                "dtUsrAuditoria": str(vtimestamp)
                                }
                    vTransaccionOrigenAux = vTransaccionOrigen + " " + str(vDatosRS[I]["dtContabilizacion"]) + " "+ str(vDatosRS[I]["idRegMovLogistico"])
                    
                vCode, vMsg = EnviarInfoWSARES(vPayLoad, vEndPoint, vUsr, vPwd, vTransaccionOrigenAux)
                vMsg = vMsg + "\n"
                vMsg = vMsg + str(vPayLoad) + "\n"                                
                EscribirLog(vMsg)
                # print(vCode, " ", vEndPoint, " ", vMsg)                            
            messagebox.showinfo(message="Termino proceso de envio de " + str(len(vDatosRS)) + " registros de " + ItpInfo + vAux, title="AppBCN")
        else:
            messagebox.showinfo(message="No existe información para la opcion: " + ItpInfo, title="AppBCN")


def getPlantillaHTML():
    vHTML = """<html><head><title>::..BCN</title></head>
 <body>
    <table cellspacing="0" cellpadding="4" border="0" style="color:#333333;font-family:Arial;font-size:X-Small;width:90%;border-collapse:collapse;">
     <tr>
       <th style="width: 10%"></th>
       <th style="width: 70%"> 
        <table cellspacing="0" cellpadding="4" border="0" style="width: 100%">
         <tr>
          <th >
            <span style="color:Black;font-family:Arial,Helvetica,Sans-serif;font-size:12pt;font-weight:bold;text-decoration:none;">GERENCIA REFINERIA BARRANCABERMEJA</span>
          </th>
         </tr>
         <tr> 
          <th><span style="color:Black;font-family:Arial,Helvetica,Sans-serif;font-size:10pt;font-weight:bold;text-decoration:none;">{InformeReporte}</span></th>
         </tr>
         <tr>
          <th><span style="color:#000066;font-family:Arial,Helvetica,Sans-serif;font-size:9pt;font-weight:bold;">Periodo {FechaIni} a {FechaFin} </span></th>
         </tr>
        </table> 
       </th>
       <th style="width: 10%"><span LineHeight="14pt" style="color:#000066;font-family:Arial,Helvetica,Sans-serif;font-size:8pt;font-weight:bold;">{TSSistema}</span></th>
     </tr>
     <tr>
      <td colspan="3">        
         {tblBalances}  
      </td>
     </tr>
    </table>
 </body>
</html>""" 
    return vHTML

def EscribirLog(vMensaje):
    # Registra información del archivo log
    vArchivoLog = open("ArcBCN.log", "a")
    vArchivoLog.write(vMensaje)
    vArchivoLog.close()

    
def oConectarDB(IDatosDB, ISql):    
    # print("datos ", IDatosDB["ServidorDB"])    
    try:                
        vMsg = "OK"
        oConnet = pymssql.connect(server = IDatosDB["ServidorDB"], port = IDatosDB["PuertoDB"], user = IDatosDB["UsrDB"], password = IDatosDB["PwdDB"], database = IDatosDB["BaseDatos"], as_dict=True, tds_version="7.0", login_timeout = 5)
        rsConsulta = oConnet.cursor()        
        rsConsulta.execute(ISql)
        if ISql[0:6].upper() == "SELECT":
            varrFilas = rsConsulta.fetchall()
        else:
            oConnet.commit()
            varrFilas = {}
        oConnet.close()
    except Exception as Ex:
        vMsg = "ERROR"
        varrFilas = {}                         
    return vMsg, varrFilas
    
def CargarConfiguracionXML(InmRutaTrabajo, InmArcXML):
    vtimestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")    
    vMsg = vtimestamp + " INFO: Se cargo con exito los parametros requeridos por APP, contenido en el archivo  [" + InmArcXML + "] de la ruta conf !!!\n"    
    try:        
        vEntrar = True
        tree = ET.parse(InmRutaTrabajo + "/conf/" + InmArcXML)                        
        root = tree.getroot()                        

        vInfoDBROMSS = root.find('Configuraciones/parametros/getInfoDBROMSS')
        vInfoDBAORA = root.find('Configuraciones/parametros/getInfoDBAORA')
        vInfoDBAORAQAS = root.find('Configuraciones/parametros/getInfoDBAORAQAS')
         
        vInfoDBBCN = root.find('Configuraciones/parametros/getInfoDBBCN')
        xmlQuerys = root.find('Configuraciones/Querys')
        
        vInfoURLSAPECCECP = root.find('Configuraciones/parametros/getInfoURLSAPECCECP')
        vInfoURLARES = root.find('Configuraciones/parametros/getInfoURLARES')
        vInfoMovCostos = root.find('Configuraciones/parametros/getJSONCostos')
        vInfoDMovLogistico = root.find('Configuraciones/parametros/getJSONMovLogistico')
        
        tkInfoDBAORA = ""
        tkInfoDBAORAQAS = ""
        tkInfoDBBCN = "" 
        tkInfoDBROMSS = ""               
        if vInfoDBROMSS.text == None:
            vEntrar = False
            vMsg = vtimestamp + " ERROR: La configuración del endpoint " + vInfoDBROMSS.attrib["Descripcion"] + " esta vacia o nula\n"
        else:            
            tkInfoDBROMSS = jwt.decode(vInfoDBROMSS.text, InmArcXML, "HS256")

        if vInfoDBAORA.text == None:
            vEntrar = False
            vMsg = vtimestamp + " ERROR: La configuración del endpoint " + vInfoDBAORA.attrib["Descripcion"] + " esta vacia o nula\n"
        else:             
            tkInfoDBAORA = jwt.decode(vInfoDBAORA.text, InmArcXML, "HS256")

        if vInfoDBAORAQAS.text == None:
            vEntrar = False
            vMsg = vtimestamp + " ERROR: La configuración del endpoint " + vInfoDBAORAQAS.attrib["Descripcion"] + " esta vacia o nula\n"
        else:            
            tkInfoDBAORAQAS = jwt.decode(vInfoDBAORAQAS.text, InmArcXML, "HS256")                                
            
        if vInfoDBBCN.text == None:
            vEntrar = False
            vMsg = vtimestamp + " ERROR: La configuración del endpoint " + vInfoDBBCN.attrib["Descripcion"] + " esta vacia o nula\n"
        else:            
            tkInfoDBBCN = jwt.decode(vInfoDBBCN.text, InmArcXML, "HS256")

        if vInfoURLARES.text == "" or vInfoURLARES.text == None:
            vEntrar = False
            vMsg = vtimestamp + " ERROR: La configuración del endpoint " + vInfoURLARES.attrib["Descripcion"] + " esta vacia o nula\n"
        else:            
            arrTokenARES = jwt.decode(vInfoURLARES.text, InmArcXML, "HS256")
        
        if vInfoURLSAPECCECP.text == "" or vInfoURLSAPECCECP.text == None:
            vEntrar = False
            vMsg = vtimestamp + " ERROR: La configuración del endpoint " + vInfoURLSAPECCECP.attrib["Descripcion"] + " esta vacia o nula\n"
        else:            
            arrTokenSAPECCECP = jwt.decode(vInfoURLSAPECCECP.text, InmArcXML, "HS256")
            
        vLstParametros =  {"txMovCostos": vInfoMovCostos.text, "txMovLogistico": vInfoDMovLogistico.text, "InfoURLARES": arrTokenARES, "InfoURLSAPECCECP": arrTokenSAPECCECP, "InfoDBROMSS": tkInfoDBROMSS, "InfoDBAORA": tkInfoDBAORA, "InfoDBAORAQAS": tkInfoDBAORAQAS, "InfoDBBCN": tkInfoDBBCN, "xQuerys": xmlQuerys}
    except FileNotFoundError:
        vLstParametros = {}
        vEntrar = False
        vMsg = vtimestamp + " ERROR: El archivo de configuracion [" + InmArcXML + "] no existe en la ruta conf de ejecucion de la app\n"
            
    return vMsg, vEntrar, vLstParametros
                
                        
def getVisualizarInfoHTML(ItpInfo, IarrParametros, ItxRutaTrabajo):
    vtimestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")        
    vtmpInvOperativo = getPlantillaHTML()
    vdtFormato = "%Y-%m-%d %H:%M:%S"
    vRGBFilaHTMLImp = "E3EAEB"
    vRGBFilaHTMLPar = "FFFFFF"
    vFuente = "GRB"        
    dtFechaIni = txtFechaIni.get() + " 00:00:00"
    dtFechaFin = txtFechaFin.get() + " 23:59:59"
    vDatosDB = IarrParametros["InfoDBBCN"]
    vtmpInvOperativo = vtmpInvOperativo.replace("{TSSistema}", str(vtimestamp)) 
    vEntrar = True
    vRespuesta = False
    vSqlAux = ""    
    # Reglas para Información Operativa 
    if ItpInfo == "InfOPerativo":
        vidCaso = 4
        vIdOpcion =  cmbIntegrarInfo.get()
        # print("algo", vIdOpcion)
        # Inventarios de AORA y ROMSS    
        if vIdOpcion[:2] in ("01", "04"):   
            vTag = "INVENTARIO OPERATIVO"    
            vRespuesta = messagebox.askyesnocancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\n Opciones: \n * Si: Fecha de Inventario Inicial: "+ str(dtFechaIni) + "\n * No: Fecha de Inventario Final :"+ str(dtFechaFin) + "\n * Cancelar: Cancelar tarea", title=vFuente + " .::AppBCN")                    
            if vRespuesta == True:
                vtmpInvOperativo = vtmpInvOperativo.replace("{FechaIni}", dtFechaIni)
                vtmpInvOperativo = vtmpInvOperativo.replace("{FechaFin}", dtFechaIni) 
                vFechaAux = datetime.strptime(dtFechaIni, vdtFormato) + timedelta(minutes = -1)
            elif vRespuesta == False:
                vtmpInvOperativo = vtmpInvOperativo.replace("{FechaIni}", dtFechaFin)
                vtmpInvOperativo = vtmpInvOperativo.replace("{FechaFin}", dtFechaFin) 
                vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds =-59)            
            elif vRespuesta is None:
                vEntrar = False
        # Movimientos y Flujos de AORA y Movimientos de ROMSS
        elif vIdOpcion[:2] in ("02", "03", "05", "08"):        
            vTagQuery = "MOVOPERAORA"
            vTag = "MOVIMIENTOS OPERATIVOS"
            vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\nPeriodo: \nDesde: "+ str(dtFechaIni) + "Hasta: "+ str(dtFechaFin), title=vFuente + " .::AppBCN")                                
            if vRespuesta == False:
                vEntrar = False                
            if vIdOpcion[:2] == "03":
                vTagQuery = "FLUOPERAORA"
                vTag = "FLUJOS OPERATIVOS"
            elif vIdOpcion[:2] == "08":
                vtpRecBalance = ""
                vTagQuery = "BALANCEOPER"
                vTag = "BALANCE OPERATIVO"
            vtmpInvOperativo = vtmpInvOperativo.replace("{FechaIni}", dtFechaIni)
            vtmpInvOperativo = vtmpInvOperativo.replace("{FechaFin}", dtFechaFin) 
        else:
            vEntrar = False            
            messagebox.showinfo(message="Esta opción no esta disponible:\n\n" + vIdOpcion, title = vFuente + " .::AppBCN")
    elif ItpInfo == "InfConsolidado":
        # Opciones de Consolidacion
        vidCaso = 5
        vIdOpcion =  cmbConsolidarInfo.get()
        if vIdOpcion[:2] == "01":
            vTag = "INVENTARIO CONSOLIDADO"
            vRespuesta = messagebox.askyesnocancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\n Opciones: \n * Si: Fecha de Inventario Inicial: "+ str(dtFechaIni) + "\n * No: Fecha de Inventario Final :"+ str(dtFechaFin) + "\n * Cancelar: Cancelar tarea", title= vFuente + " .::AppBCN")                                
            if vRespuesta == True:
                vtmpInvOperativo = vtmpInvOperativo.replace("{FechaIni}", dtFechaIni)
                vtmpInvOperativo = vtmpInvOperativo.replace("{FechaFin}", dtFechaIni)
                vFechaAux = datetime.strptime(dtFechaIni, vdtFormato) + timedelta(minutes = -1)
            elif vRespuesta == False:
                vtmpInvOperativo = vtmpInvOperativo.replace("{FechaIni}", dtFechaFin)
                vtmpInvOperativo = vtmpInvOperativo.replace("{FechaFin}", dtFechaFin) 
                vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds =-59)            
            elif vRespuesta is None:
                vEntrar = False 
        elif vIdOpcion[:2] == "02":
            vTag = "MOVIMIENTOS CONSOLIDADOS"
            vtmpInvOperativo = vtmpInvOperativo.replace("{FechaIni}", dtFechaIni)
            vtmpInvOperativo = vtmpInvOperativo.replace("{FechaFin}", dtFechaFin) 
        elif vIdOpcion[:2] in ("06", "07", "08", "09"): # Opc Dif Balance
            vEntrar = False            
            messagebox.showinfo(message="Esta opción no esta disponible:\n\n" + vIdOpcion, title = vFuente + " .::AppBCN")
        else:
            vtmpInvOperativo = vtmpInvOperativo.replace("{FechaIni}", dtFechaIni)
            vtmpInvOperativo = vtmpInvOperativo.replace("{FechaFin}", dtFechaFin)
            vLstBalance = ['ALMACEN PRODUCTO', 'POOL PRODUCTO', 'UNIDAD DE PROCESO']             
            vtpRecBalance = vLstBalance[int(vIdOpcion[:2])-3]
            vTagQuery = "GETBALANCECONSBCN"
            if vIdOpcion[:2] == "03":                
                vTag = "BALANCE CONSOLIDADO POR ALMACEN"
            elif vIdOpcion[:2] == "04":
                vTag = "BALANCE CONSOLIDADO POR POOL"
            elif vIdOpcion[:2] == "05":
                vTag = "BALANCE CONSOLIDADO POR UNIDAD DE PROCESO"
            elif vIdOpcion[:2] == "06":
                vTag = "FOTO DE INENTARIO CONSOLIDADO"
                vEntrar = False
                
    elif ItpInfo == "InfBalance":
        # Opciones de Balance Logistico
        vSqlAux = ""
        vIdOpcion =  cmbBalanceInfo.get()
        vtmpInvOperativo = vtmpInvOperativo.replace("{FechaIni}", dtFechaIni)
        vtmpInvOperativo = vtmpInvOperativo.replace("{FechaFin}", dtFechaFin)   
        vCantRegistros = 1
        vTag = "BALANCE DEL CENTRO LOGISTICO " + str(vIdOpcion[-4:])
        vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\nPeriodo: \nDesde: "+ str(dtFechaIni) + "Hasta: "+ str(dtFechaFin) , title=vFuente + " .::AppBCN")                    
        if vRespuesta == False:
            vEntrar = False                
        if vIdOpcion[:2] == "01":        
            vTag = "MOVIMIENTOS LOGISTICOS"
        if vIdOpcion[:2] == "02":        
            vTag = "MOVIMIENTOS DE COSTOS"
            vEntrar = False            
            
    if vEntrar == True:            
        vtmpInvOperativo = vtmpInvOperativo.replace("{InformeReporte}", vTag)    
        if vTag[:10] == "INVENTARIO":
            vTablaHML = """<table cellspacing="0" cellpadding="4" border="0" style="color:#333333;font-family:Arial;font-size:X-Small;width:100%;border-collapse:collapse;">                   
                        <tr style="color:#000066; background-color:#C9D200;font-weight:bold;">
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:2%;">Item</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:15%;">Producto</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:13%;">Almancen</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Foto&nbsp;Inv.</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">VoBo</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">API</td>                    
                            <td colspan=4 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Volumen</td> 
                            <td colspan=4 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Masa</td>    
                            <td colspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Muestra</td>                   
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Estado</td>
                        </tr>
                        <tr style="color:#000066; background-color:#C9D200;font-weight:bold;">                    
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Total</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Bombeable</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Remanente</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:3%;">UM</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Total</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Bombeable</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Remanente</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:3%;">UM</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">ID.</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Fecha</td>
                        </tr>
                        {itemsFilHTML}          
                        </table>"""
                
            vFilitemHTML = """<tr align="left" style="background-color:#{RGBFila};">
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{Item}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{nmProducto}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{Almacen}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{FotoInv}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{VoBo}</td>
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{API}</td>
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{TotalNSV}</td>
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{BombeableNSV}</td>
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{RemanenteNSV}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{UMVol}</td>                                    
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{TotalNSW}</td>
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{BombeableNSW}</td>                  
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{RemanenteNSW}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{UMMas}</td>                                    
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{nbMuestra}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{dtMuestra}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{nmEstado}</td>
                        </tr>"""                 
            
            vSql = IarrParametros["xQuerys"].find("qryGETINVENTARIOSBCN")             
            vSqlAux = vSql.text.strip() 
            vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(vFechaAux))
            vSqlAux = vSqlAux.replace('[idCaso]', str(vidCaso))     
        elif (vTag[:11] == "MOVIMIENTOS" or vTag[:6] == "FLUJOS") and ItpInfo != "InfBalance":
            
            # nbMovimientoTag, tpMovimientoCls, dtMovimientoIni, dtMovimientoFin, nmRecOrigen, nmProdOrigen, nmRecDestino, nmProdDestino, 
            # vlCantVolFuente, vlCantVolReconciliado, vlCantVolConciliado, idUMCantVol, vlCantMasFuente, vlCantMasReconciliado, vlCantMasConciliado, idUMCantMas
            # , nbAPI60, nbNumMuestra, dtUltMuestra, numPedido, posPedido, dtCargado, nmUsrAuditoria
            vTablaHML = """<table cellspacing="0" cellpadding="4" border="0" style="color:#333333;font-family:Arial;font-size:X-Small;width:100%;border-collapse:collapse;">                   
                        <tr style="color:#000066; background-color:#C9D200;font-weight:bold;">
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:2%;">Item</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:15%;">Tag</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:13%;">Tipo&nbsp;Mov.</td>                                                
                            <td colspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Fecha</td>
                            <td colspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Origen</td>
                            <td colspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Destino</td>
                            <td colspan=4 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Volumen</td>
                            <td colspan=4 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Masa</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">API</td>                         
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">ID. Muestra</td>
                            <td colspan=3 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Pedido</td>                        
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Estado</td>
                        </tr>
                        <tr style="color:#000066; background-color:#C9D200;font-weight:bold;">
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Inicio</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Fin</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Recurso</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Producto</td>                        
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Recurso</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Producto</td>                                            
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Fuente</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Reconciliado</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Conciliado</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:3%;">UM</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Fuente</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Reconciliado</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Conciliado</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:3%;">UM</td>                        
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Numero.</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Posici&oacuten</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">UM</td>
                        </tr>
                        {itemsFilHTML}          
                        </table>"""
                        
            vFilitemHTML = """<tr align="left" style="background-color:#{RGBFila};">
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{Item}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{Tag}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{tpMov}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{FIni}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{FFin}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{RecOrigen}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{ProdOrigen}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{RecDestino}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{ProdDestino}</td>                    
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{FuenteVol}</td>
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{ReconciliadoVol}</td>                 
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{ConciliadoVol}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{UMVol}</td>
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{FuenteMas}</td>
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{ReconciliadoMas}</td>                 
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{ConciliadoMas}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{UMMas}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{API}</td>                        
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{nbMuestra}</td>                        
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{NumPedido}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{PosPedido}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{idUMPedido}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{nmEstado}</td>
                        </tr>"""       
            vFiltro = "<>"
            tpMov = "LIMBAT"
            if vIdOpcion[:2] == "03":    
                vFiltro = "="
            vSql = IarrParametros["xQuerys"].find("qryGETMOVIMIENTOSBCN")
            vSqlAux = vSql.text.strip() 
            vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(dtFechaIni))       
            vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(dtFechaFin))      
            vSqlAux = vSqlAux.replace('[Filtro01]', vFiltro)
            vSqlAux = vSqlAux.replace('[tpMovimiento]', tpMov)
            vSqlAux = vSqlAux.replace('[idCaso]', str(vidCaso))
            # print(vSqlAux)
        elif vTag[:7] == "BALANCE" and ItpInfo != "InfBalance":
            vTablaHML = """<table cellspacing="0" cellpadding="4" border="0" style="color:#333333;font-family:Arial;font-size:X-Small;width:100%;border-collapse:collapse;">                   
                        <tr style="color:#000066; background-color:#C9D200;font-weight:bold;">
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:2%;">Item</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:10%;">ID</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:10%;">Codigo</td>                            
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:30%;">Recurso</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:13%;">UM</td>                                                                            
                            <td colspan=6 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Volumen</td>
                            <td colspan=6 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Masa</td>                            
                        </tr>
                        <tr style="color:#000066; background-color:#C9D200;font-weight:bold;">
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Inv.&nbsp;Inicial</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Entradas</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Salidas</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Inv.&nbsp;Final</td>                        
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Desbalance</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">UM</td>                                            
                            
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Inv.&nbsp;Inicial</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Entradas</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Salidas</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Inv.&nbsp;Final</td>                        
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Desbalance</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">UM</td>                                            
                        </tr>         
                        {itemsFilHTML}          
                        </table>"""
            
            vFilitemHTML = """<tr align="left" style="background-color:#{RGBFila};">
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{Item}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{ID}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{CodSAP}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{nmRecurso}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{UMBal}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{InvIniVol}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{EntVol}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{SalVol}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{InvFinVol}</td>                    
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{DesbalanceVol}</td>                    
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{UMVol}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{InvIniMas}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{EntMas}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{SalMas}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{InvFinMas}</td>                    
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{DesbalanceMas}</td>                    
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{UMMas}</td>                                                
                        </tr>"""                        
            
            vSql = IarrParametros["xQuerys"].find("qry" + vTagQuery)
            vSqlAux = vSql.text.strip() 
            vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(dtFechaIni))
            vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(dtFechaFin))                  
            vSqlAux = vSqlAux.replace('[tpMovimiento]', vtpRecBalance)

        elif vTag[:11] == "MOVIMIENTOS" and ItpInfo == "InfBalance":
            
            # nbMovimientoTag, tpMovimientoCls, dtMovimientoIni, dtMovimientoFin, nmRecOrigen, nmProdOrigen, nmRecDestino, nmProdDestino, 
            # vlCantVolFuente, vlCantVolReconciliado, vlCantVolConciliado, idUMCantVol, vlCantMasFuente, vlCantMasReconciliado, vlCantMasConciliado, idUMCantMas
            # , nbAPI60, nbNumMuestra, dtUltMuestra, numPedido, posPedido, dtCargado, nmUsrAuditoria
            vTablaHML = """<table cellspacing="0" cellpadding="4" border="0" style="color:#333333;font-family:Arial;font-size:X-Small;width:100%;border-collapse:collapse;">                   
                        <tr style="color:#000066; background-color:#C9D200;font-weight:bold;">
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:2%;">Item</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:15%;">Tag</td>
                            <td colspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:13%;">Clase&nbsp;Movimiento</td>                                                
                            <td colspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Fecha</td>
                            <td colspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Origen</td>
                            <td colspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Destino</td>
                            <td colspan=3 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Contabilizacion</td>                            
                            <td colspan=3 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Pedido</td>                        
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">CeCo</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Estado</td>
                        </tr>
                        <tr style="color:#000066; background-color:#C9D200;font-weight:bold;">    
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">ID</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Descripcin</td>
                                                    
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Inicio</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Fin</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Recurso</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Producto</td>                        
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Recurso</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Producto</td>                                            
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Fecha</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Valor</td>                            
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:3%;">UM</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Numero.</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Posici&oacuten</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">UM</td>
                        </tr>
                        {itemsFilHTML}          
                        </table>"""
                        
            vFilitemHTML = """<tr align="left" style="background-color:#{RGBFila};">
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{Item}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{Tag}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{tpMov}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{txMov}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{FIni}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{FFin}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{RecOrigen}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{ProdOrigen}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{RecDestino}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{ProdDestino}</td>                    
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{dtContable}</td>
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{vlContable}</td>                                         
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{UMContable}</td>                        
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{NumPedido}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{PosPedido}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{idUMPedido}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{nbCeCo}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{nmEstado}</td>
                        </tr>"""                               
            vSql = IarrParametros["xQuerys"].find("qryWSMOVLOGISTICO")
            vSqlAux = vSql.text.strip() 
            vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(dtFechaIni))       
            vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(dtFechaFin))      
            vSqlAux = vSqlAux.replace('[Filtro]', '')             
            # print(vSqlAux)
        elif vTag[:7] == "BALANCE" and ItpInfo == "InfBalance":
            vTablaHML = """<table cellspacing="0" cellpadding="4" border="0" style="color:#333333;font-family:Arial;font-size:X-Small;width:100%;border-collapse:collapse;">                   
                        <tr style="color:#000066; background-color:#C9D200;font-weight:bold;">
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:2%;">Item</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:10%;">ID</td>
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:10%;">Codigo</td>                            
                            <td rowspan=2 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:30%;">Recurso</td>
                            <td colspan=6 align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:7%;">Balance</td>
                        </tr>
                        <tr style="color:#000066; background-color:#C9D200;font-weight:bold;">
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Inv.&nbsp;Inicial</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:8%;">Entradas</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Salidas</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Inv.&nbsp;Final</td>                        
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">Desbalance</td>
                            <td align="center" style="border-color:Black;border-width:1px;border-style:Solid; width:5%;">UM</td>                                                                        
                        </tr>
                        {itemsFilHTML}          
                        </table>"""
                        
            vFilitemHTML = """<tr align="left" style="background-color:#{RGBFila};{NegritaFila}">
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{Item}</td>
                        <td align="center" style="border-color:Black;border-width:1px;border-style:Solid;">{ID}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{CodSAP}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{nmRecurso}</td>
                        
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{InvIni}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{Entradas}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{Salidas}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{InvFin}</td>
                        <td align="left" style="border-color:Black;border-width:1px;border-style:Solid;">{Desbalance}</td>
                        <td align="right" style="border-color:Black;border-width:1px;border-style:Solid;">{UM}</td>
                        </tr>"""                      
            vCeLo = vIdOpcion[-4:]          
            vSql = IarrParametros["xQuerys"].find("qryBALLOGISTICO") 
            vSqlAux = vSql.text.strip() 
            vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds =1)
            vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(dtFechaIni))
            vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(vFechaAux))                              
            vSqlAux = vSqlAux.replace('[nbCeLo]', str(vCeLo))                              
                    
        # print("Query ", vSqlAux)                                
        # vFechaAux = datetime.strptime(dtFechaIni, vdtFormato) + timedelta(seconds = -1)    
        vTablas = ""
        vFilasTablasHTML = ""
        if len(vSqlAux) > 0:
            vMsg, vDatosRS = oConectarDB(vDatosDB, vSqlAux)                
        
        if len(vDatosRS):        
            for I in range(len(vDatosRS)):
                vCad = vFilitemHTML
                vFILRGB = vRGBFilaHTMLImp
                if I % 2 == 0:
                    vFILRGB = vRGBFilaHTMLPar
                vCad = vCad.replace('{RGBFila}', vFILRGB)
                
                vCad = vCad.replace('{Item}', str(I+1))
                if vTag[:10] == "INVENTARIO":
                    vCad = vCad.replace('{nmProducto}', str(vDatosRS[I]["nmRecProducto"]))
                    vCad = vCad.replace('{Almacen}', str(vDatosRS[I]["nmRecAlmacen"]))
                    vCad = vCad.replace('{VoBo}', str(vDatosRS[I]["boVoBoAlmacen"]))
                    vCad = vCad.replace('{FotoInv}', str(vDatosRS[I]["boFotoInventario"]))
                    vCad = vCad.replace('{API}', str(vDatosRS[I]["nbAPI60"]))
                    vCad = vCad.replace('{TotalNSV}', str(vDatosRS[I]["CantVolTotal"]))
                    vCad = vCad.replace('{BombeableNSV}', str(vDatosRS[I]["CantVolBombeable"]))
                    vCad = vCad.replace('{RemanenteNSV}', str(vDatosRS[I]["CantVolRemanente"]))
                    vCad = vCad.replace('{UMVol}', str(vDatosRS[I]["idUMVolumen"]))
                    vCad = vCad.replace('{TotalNSW}', str(vDatosRS[I]["CantMasTotal"]))
                    vCad = vCad.replace('{BombeableNSW}', str(vDatosRS[I]["CantMasBombeable"]))
                    vCad = vCad.replace('{RemanenteNSW}', str(vDatosRS[I]["CantMasRemanente"]))
                    vCad = vCad.replace('{UMMas}', str(vDatosRS[I]["idUMMasa"]))
                    vCad = vCad.replace('{nbMuestra}', str(vDatosRS[I]["nbMuestra"]))
                    vCad = vCad.replace('{dtMuestra}', str(vDatosRS[I]["dtMuestra"]))
                    vCad = vCad.replace('{nmEstado}', str(vDatosRS[I]["nmEstado"]))
                elif (vTag[:11] == "MOVIMIENTOS" or vTag[:6] == "FLUJOS") and ItpInfo != "InfBalance":
                    vCad = vCad.replace('{Tag}', str(vDatosRS[I]["nbMovimientoTag"]))
                    vCad = vCad.replace('{tpMov}', str(vDatosRS[I]["tpMovimientoCls"]))
                    vCad = vCad.replace('{FIni}', str(vDatosRS[I]["dtMovimientoIni"]))
                    vCad = vCad.replace('{FFin}', str(vDatosRS[I]["dtMovimientoFin"]))
                    vCad = vCad.replace('{RecOrigen}', str(vDatosRS[I]["nmRecOrigen"]))
                    vCad = vCad.replace('{ProdOrigen}', str(vDatosRS[I]["nmProdOrigen"]))
                    vCad = vCad.replace('{RecDestino}', str(vDatosRS[I]["nmRecDestino"]))
                    vCad = vCad.replace('{ProdDestino}', str(vDatosRS[I]["nmProdDestino"]))                
                    vCad = vCad.replace('{FuenteVol}', str(vDatosRS[I]["vlCantVolFuente"]))
                    vCad = vCad.replace('{ReconciliadoVol}', str(vDatosRS[I]["vlCantVolReconciliado"]))
                    vCad = vCad.replace('{ConciliadoVol}', str(vDatosRS[I]["vlCantVolConciliado"]))
                    vCad = vCad.replace('{UMVol}', str(vDatosRS[I]["idUMCantVol"]))                
                    vCad = vCad.replace('{FuenteMas}', str(vDatosRS[I]["vlCantMasFuente"]))
                    vCad = vCad.replace('{ReconciliadoMas}', str(vDatosRS[I]["vlCantMasReconciliado"]))
                    vCad = vCad.replace('{ConciliadoMas}', str(vDatosRS[I]["vlCantMasConciliado"]))
                    vCad = vCad.replace('{UMMas}', str(vDatosRS[I]["idUMCantMas"]))
                    vCad = vCad.replace('{API}', str(vDatosRS[I]["nbAPI60"]))
                    vCad = vCad.replace('{nbMuestra}', str(vDatosRS[I]["nbMuestra"]))                    
                    vCad = vCad.replace('{NumPedido}', str(vDatosRS[I]["numPedido"]))
                    vCad = vCad.replace('{PosPedido}', str(vDatosRS[I]["posPedido"]))
                    vCad = vCad.replace('{idUMPedido}', str(vDatosRS[I]["idUMPedido"]))
                    vCad = vCad.replace('{nmEstado}', str(vDatosRS[I]["nmEstado"]))
                elif vTag[:11] == "MOVIMIENTOS" and ItpInfo == "InfBalance":
                    if vTag == "MOVIMIENTOS LOGISTICOS":
                        vCad = vCad.replace('{Tag}', str(vDatosRS[I]["idRegMovLogistico"]))
                        vCad = vCad.replace('{tpMov}', str(vDatosRS[I]["nbMovimientoCls"]))
                        vCad = vCad.replace('{txMov}', str(vDatosRS[I]["nmMovimientoCls"]))
                        vCad = vCad.replace('{FIni}', str(vDatosRS[I]["dtMovimientoIni"]))
                        vCad = vCad.replace('{FFin}', str(vDatosRS[I]["dtMovimientoFin"]))
                        vCad = vCad.replace('{RecOrigen}', str(vDatosRS[I]["nmAlmLogOrigen"]))
                        vCad = vCad.replace('{ProdOrigen}', str(vDatosRS[I]["nmProdLogOrigen"]))
                        vCad = vCad.replace('{RecDestino}', str(vDatosRS[I]["nmAlmLogDestino"]))
                        vCad = vCad.replace('{ProdDestino}', str(vDatosRS[I]["nmProdLogDestino"]))                
                        vCad = vCad.replace('{dtContable}', str(vDatosRS[I]["dtContabilizacion"]))
                        vCad = vCad.replace('{vlContable}', str(vDatosRS[I]["vlContable"]))
                        vCad = vCad.replace('{UMContable}', str(vDatosRS[I]["idUM"]))  
                        vCad = vCad.replace('{NumPedido}', str(vDatosRS[I]["numPedido"]))
                        vCad = vCad.replace('{PosPedido}', str(vDatosRS[I]["posPedido"]))
                        vCad = vCad.replace('{idUMPedido}', str(vDatosRS[I]["idUMPedido"]))
                        vCad = vCad.replace('{nbCeCo}', str(vDatosRS[I]["nbCentroCosto"]))
                        vCad = vCad.replace('{nmEstado}', str(vDatosRS[I]["txProcesamiento"]))
                    
                elif vTag[:7] == "BALANCE" and (ItpInfo != "InfBalance" or ItpInfo == "InfOPerativo"):
                    vCad = vCad.replace('{ID}', str(vDatosRS[I]["idRecurso"]))
                    vCad = vCad.replace('{CodSAP}', str(vDatosRS[I]["nbRecurso"]))
                    vCad = vCad.replace('{nmRecurso}', str(vDatosRS[I]["nmRecurso"]))
                    vCad = vCad.replace('{UMBal}', str(vDatosRS[I]["UMBalance"]))                                        				                    
                    vCad = vCad.replace('{InvIniVol}', str(vDatosRS[I]["InvIniVol"]))
                    vCad = vCad.replace('{EntVol}', str(vDatosRS[I]["vlEntVol"]))
                    vCad = vCad.replace('{SalVol}', str(vDatosRS[I]["vlSalVol"]))
                    vCad = vCad.replace('{InvFinVol}', str(vDatosRS[I]["InvFinVol"]))
                    vCad = vCad.replace('{DesbalanceVol}', str(vDatosRS[I]["vlDesbalanceVol"]))
                    vCad = vCad.replace('{UMVol}', str(vDatosRS[I]["UMVol"]))
                    vCad = vCad.replace('{InvIniMas}', str(vDatosRS[I]["InvIniMas"]))
                    vCad = vCad.replace('{EntMas}', str(vDatosRS[I]["vlEntMas"]))
                    vCad = vCad.replace('{SalMas}', str(vDatosRS[I]["vlSalMas"]))
                    vCad = vCad.replace('{InvFinMas}', str(vDatosRS[I]["InvFinMas"]))
                    vCad = vCad.replace('{DesbalanceMas}', str(vDatosRS[I]["vlDesbalanceMas"]))
                    vCad = vCad.replace('{UMMas}', str(vDatosRS[I]["UMMas"]))
                    
                elif vTag[:7] == "BALANCE" and ItpInfo == "InfBalance":
                    vAux = ""
                    if vDatosRS[I]["Nivel"] < 3:                        
                        vAux = "font-weight: bold;" 
                    vCad = vCad.replace('{NegritaFila}', vAux)
                    vCad = vCad.replace('{ID}', str(vDatosRS[I]["idRecurso"]))
                    vCad = vCad.replace('{CodSAP}', str(vDatosRS[I]["nbRecurso"]))
                    vCad = vCad.replace('{nmRecurso}', str(vDatosRS[I]["nmRecurso"]))                    
                    vCad = vCad.replace('{InvIni}', str(vDatosRS[I]["InvIni"]))
                    vCad = vCad.replace('{Entradas}', str(vDatosRS[I]["vlEntradas"]))
                    vCad = vCad.replace('{Salidas}', str(vDatosRS[I]["vlSalidas"]))
                    vCad = vCad.replace('{InvFin}', str(vDatosRS[I]["InvFin"]))
                    vCad = vCad.replace('{Desbalance}', str(vDatosRS[I]["vlDesbalance"]))
                    vCad = vCad.replace('{UM}', str(vDatosRS[I]["UM"]))
                    
                vFilasTablasHTML = vFilasTablasHTML + vCad
        
        # vFilasTablasHTML = vTablaHML.replace('{itemsFilHTML}', vFilasTablasHTML)
        # vtmpInvOperativo = vtmpInvOperativo.replace('{tblBalances}', vFilasTablasHTML)         
        # vTablas = vTablas.replace('{tblBalances}', vFilasTablasHTML)
        vTablaHML = vTablaHML.replace('{itemsFilHTML}', vFilasTablasHTML)
        vtmpInvOperativo = vtmpInvOperativo.replace('{tblBalances}', vTablaHML)
        
        vArcInventarioOper = ItxRutaTrabajo + "/conf/repOperativo.html" 
        ArcInvOperativo = open(vArcInventarioOper, "w")
        ArcInvOperativo.write(vtmpInvOperativo)
        # ArcBalProductos.write(vtmpBalProd)
        ArcInvOperativo.close()            
        os.startfile(vArcInventarioOper)
    else:
        if vRespuesta == True:
            messagebox.showinfo(message="No existe información o Funcionalidad no disponible para esta opción:     \n\n" + vIdOpcion, title = vFuente + ".::AppBCN")

def getVisualizarInfoExcel(ItpInfo, IarrParametros, ItxRutaTrabajo):
    vtimestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")            
    vdtFormato = "%Y-%m-%d %H:%M:%S"
    vFuente = "GRB"        
    dtFechaIni = txtFechaIni.get() + " 00:00:00"
    dtFechaFin = txtFechaFin.get() + " 23:59:59"
    vDatosDB = IarrParametros["InfoDBBCN"]    
    vEntrar = True
    if ItpInfo == "InfOPerativo":
        vidCaso = 4
        vIdOpcion =  cmbIntegrarInfo.get()    
        # Inventarios de AORA y ROMSS    
        if vIdOpcion[:2] in ("01", "04"):        
            vTagQuery = "GETINVENTARIOSBCN"
            vTag = "INVENTARIO OPERATIVO"    
            vRespuesta = messagebox.askyesnocancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\n Opciones: \n * Si: Fecha de Inventario Inicial: "+ str(dtFechaIni) + "\n * No: Fecha de Inventario Final :"+ str(dtFechaFin) + "\n * Cancelar: Cancelar tarea", title=vFuente + " .::AppBCN")
            if vRespuesta == True:
                vFechaAux = datetime.strptime(dtFechaIni, vdtFormato) + timedelta(minutes = -1)
            elif vRespuesta == False:
                vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds =-59)            
            elif vRespuesta is None:
                vEntrar = False
        # Movimientos y Flujos de AORA y Movimientos de ROMSS
        elif vIdOpcion[:2] in ("02", "03", "05", "07", "08"):
            vTagQuery = "MOVOPERAORA"
            vTag = "MOVIMIENTOS OPERATIVOS"
            vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\nPeriodo: \nDesde: "+ str(dtFechaIni) + " Hasta: "+ str(dtFechaFin), title=vFuente + " .::AppBCN")                    
            if vRespuesta == False:
                vEntrar = False                                
            if vIdOpcion[:2] == "03":
                vTagQuery = "FLUOPERAORA"
                vTag = "FLUJOS OPERATIVOS"
            elif vIdOpcion[:2] == "08":
                vtpRecBalance = ""
                vTagQuery = "BALANCEOPER"
                vTag = "BALANCE OPERATIVO"

        elif vIdOpcion[:2] == "06":
            vTagQuery = "GETINVFOTOBCN"            
            vTag = "INVENTARIO FOTO"            
            vRespuesta = messagebox.askyesnocancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\n Opciones: \n * Si: Fecha de Inventario Inicial: "+ str(dtFechaIni) + "\n * No: Fecha de Inventario Final :"+ str(dtFechaFin) + "\n * Cancelar: Cancelar tarea", title=vFuente + " .::AppBCN")                    
            if vRespuesta == True:
                vFechaAux = datetime.strptime(dtFechaIni, vdtFormato) + timedelta(minutes = -1)
            elif vRespuesta == False:
                vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds =-59)            
            elif vRespuesta is None:
                vEntrar = False
                                  
    elif ItpInfo == "InfConsolidado":
        # Opciones de Consolidacion
        vidCaso = 5
        vIdOpcion =  cmbConsolidarInfo.get()
        if vIdOpcion[:2] in ("01", "06"):
            vTagQuery = "GETINVENTARIOSBCN"
            # vTagQuery = "INVCONSOLIDADO"
            vTag = "INVENTARIO CONSOLIDADO"
            if vIdOpcion[:2] == "06":
                vTagQuery = "GETFOTOINVCONSBCN"
                vTag = "FOTO DE INVENTARIOS CONSOLIDADO"   
            vRespuesta = messagebox.askyesnocancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\n Opciones: \n * Si: Fecha de Inventario Inicial: "+ str(dtFechaIni) + "\n * No: Fecha de Inventario Final :"+ str(dtFechaFin) + "\n * Cancelar: Cancelar tarea", title= vFuente + " .::AppBCN")                    
            if vRespuesta == True:
                vFechaAux = datetime.strptime(dtFechaIni, vdtFormato) + timedelta(minutes = -1)
            elif vRespuesta == False:
                vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds =-59)            
            elif vRespuesta is None:
                vEntrar = False
                          
        elif vIdOpcion[:2] == "02":            
            vTagQuery = "GETMOVIMIENTOSBCN"        
            vTag = "MOVIMIENTOS CONSOLIDADOS"
            vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\nPeriodo: \nDesde: "+ str(dtFechaIni) + " Hasta: "+ str(dtFechaFin), title=vFuente + " .::AppBCN")                    
            if vRespuesta == False:
                vEntrar = False
        elif vIdOpcion[:2] in ("07", "08", "09"): # Opc Dif Balance
            vEntrar = False            
            messagebox.showinfo(message="Esta opcion no esta disponible.\n\n" + vIdOpcion, title= vFuente +" .::AppBCN")
        else:
            vLstBalance = ['ALMACEN PRODUCTO', 'POOL PRODUCTO', 'UNIDAD DE PROCESO', 'FOTO DE INVENTARIO CONSOLIDADO'] 
            vtpRecBalance = vLstBalance[int(vIdOpcion[:2])-3]                                    
            vTagQuery = "GETBALANCECONSBCN"            
            vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\nPeriodo: \nDesde: "+ str(dtFechaIni) + " Hasta: "+ str(dtFechaFin), title=vFuente + " .::AppBCN")                    
            if vRespuesta == False:
                vEntrar = False
            if vIdOpcion[:2] == "03":
                vTag = "BALANCE CONSOLIDADO POR ALMACEN"
            elif vIdOpcion[:2] == "04":
                vTag = "BALANCE CONSOLIDADO POR POOL"
            elif vIdOpcion[:2] == "05":
                vTag = "BALANCE CONSOLIDADO POR UNIDAD DE PROCESO"                                                  
    elif ItpInfo == "InfBalance":
        # Opciones de Balance Logistico
        vSqlAux = ""
        vIdOpcion =  cmbBalanceInfo.get()
        vCantRegistros = 1
        vtpRecBalance = ""
        vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\nPeriodo: \nDesde: "+ str(dtFechaIni) + " Hasta: "+ str(dtFechaFin), title=vFuente + " .::AppBCN")                    
        if vRespuesta == False:
            vEntrar = False        
        if vIdOpcion[:2] == "01":        
            vTag = "MOVIMIENTOS LOGISTICOS"
            vTagQuery = "WSMOVLOGISTICO"            
        elif vIdOpcion[:2] == "02":        
            vTag = "MOVIMIENTOS DE COSTOS"
            vTagQuery = "WSCOSTOS"
        elif vIdOpcion[:2] in ("03", "04", "05"):        
            vTag = "BALANCE LOGISTICO"
            vTagQuery = "BALLOGISTICO"
    elif ItpInfo == "InfEnviosWS":
        # Opciones de Balance Logistico
        vSqlAux = ""
        vIdOpcion = cmbEnvioWSARESInfo.get()                        
        vCantRegistros = 1
        vtpRecBalance = ""
        vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\nPeriodo: \nDesde: "+ str(dtFechaIni) + " Hasta: "+ str(dtFechaFin), title=vFuente + " .::AppBCN")                    
        if vRespuesta == False:
            vEntrar = False                    
        if vIdOpcion[:2] == "01":
            vTag = "LOGINVENTARIOS"
            vTagQuery = "WSINVLOGISTICO"    
        elif vIdOpcion[:2] == "02":  
            vTag = "LOGMOVIMIENTOS"
            vTagQuery = "WSMOVLOGISTICO"        
        elif vIdOpcion[:2] == "03":  
            vTag = "LOGCOSTOS"
            vTagQuery = "WSCOSTOS"  
        elif vIdOpcion[:2] == "06":  
            vTag = "COMPARAINVENTARIO"
            vTagQuery = "COMPARAINVENTARIO"
        elif vIdOpcion[:2] == "07":  
            vTag = "COMPARACOSTO"
            vTagQuery = "COMPARACOSTO"
                                          
    if vEntrar == True:                    
        if vTag[:10] == "INVENTARIO" or vTag[:4] == "FOTO":            
            vSql = IarrParametros["xQuerys"].find("qry"+vTagQuery)             
            vSqlAux = vSql.text.strip()
            vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(vFechaAux))
            vSqlAux = vSqlAux.replace('[idCaso]', str(vidCaso))                    
        elif vTag == "LOGINVENTARIOS" or vTag == "COMPARAINVENTARIO":
            vSql = IarrParametros["xQuerys"].find("qry" + vTagQuery)             
            vSqlAux = vSql.text.strip()
            vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds =-59)        
            vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(vFechaAux))
        elif (vTag[:11] == "MOVIMIENTOS" or vTag[:6] == "FLUJOS") and ItpInfo != "InfBalance":
            # nbMovimientoTag, tpMovimientoCls, dtMovimientoIni, dtMovimientoFin, nmRecOrigen, nmProdOrigen, nmRecDestino, nmProdDestino, 
            # vlCantVolFuente, vlCantVolReconciliado, vlCantVolConciliado, idUMCantVol, vlCantMasFuente, vlCantMasReconciliado, vlCantMasConciliado, idUMCantMas
            # , nbAPI60, nbNumMuestra, dtUltMuestra, numPedido, posPedido, dtCargado, nmUsrAuditoria
            vFiltro = "<>"
            tpMov = "LIMBAT"
            if vIdOpcion[:2] == "03":    
                vFiltro = "="
            vSql = IarrParametros["xQuerys"].find("qryGETMOVIMIENTOSBCN")
            vSqlAux = vSql.text.strip() 
            vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(dtFechaIni))       
            vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(dtFechaFin))      
            vSqlAux = vSqlAux.replace('[Filtro01]', vFiltro)
            vSqlAux = vSqlAux.replace('[tpMovimiento]', tpMov)
            vSqlAux = vSqlAux.replace('[idCaso]', str(vidCaso))
            # print(vSqlAux)
        elif vTag[:11] == "MOVIMIENTOS" and ItpInfo == "InfBalance":
            vSql = IarrParametros["xQuerys"].find("qry"+vTagQuery)
            vSqlAux = vSql.text.strip() 
            vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(dtFechaIni))
            vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(dtFechaFin))           
            vSqlAux = vSqlAux.replace('[Filtro]', '')
            # print(vSqlAux)                                
        elif vTag[:7] == "BALANCE" or vTag == "LOGMOVIMIENTOS" or vTag == "LOGCOSTOS":
            vCeLo = vIdOpcion[-4:]
            vSql = IarrParametros["xQuerys"].find("qry"+vTagQuery)
            vSqlAux = vSql.text.strip() 
            vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(dtFechaIni))
            vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(dtFechaFin))                  
            vSqlAux = vSqlAux.replace('[nbCeLo]', str(vCeLo)) 
            vSqlAux = vSqlAux.replace('[tpMovimiento]', vtpRecBalance)
            vSqlAux = vSqlAux.replace('[Filtro]', '')

        # print("Query ", vSqlAux)                                
        # vFechaAux = datetime.strptime(dtFechaIni, vdtFormato) + timedelta(seconds = -1)    
        vMsg, vDatosRS = oConectarDB(vDatosDB, vSqlAux)                        
        if len(vDatosRS):        
            arrdata = []
            arrEtiquetas = []
            for I in range(len(vDatosRS)):
                arrFilData = []    
                for key, value in vDatosRS[I].items():
                    arrFilData.append(value)
                    if I == 0:
                        arrEtiquetas.append(key)
                arrdata.append(arrFilData)
            try: 
                InmExcel = vTagQuery + ".xlsx"
                df = pd.DataFrame(arrdata, columns=arrEtiquetas)
                # cOMVIERTE A NUMERO el valor 
                if vTag == "INVENTARIO OPERATIVO":
                    df["nbAPI60"] = pd.to_numeric(df["nbAPI60"])
                    df["CantVolTotal"] = pd.to_numeric(df["CantVolTotal"])
                    df["CantVolBombeable"] = pd.to_numeric(df["CantVolBombeable"])
                    df["CantVolRemanente"] = pd.to_numeric(df["CantVolRemanente"])
                    df["CantMasTotal"] = pd.to_numeric(df["CantMasTotal"])
                    df["CantMasBombeable"] = pd.to_numeric(df["CantMasBombeable"])
                    df["CantMasRemanente"] = pd.to_numeric(df["CantMasRemanente"])                    
                elif vTag == "INVENTARIO FOTO":
                    df["CantTotal"] = pd.to_numeric(df["CantTotal"])
                    df["CantBombeableLU"] = pd.to_numeric(df["CantBombeableLU"])
                    df["CantBombeableCC"] = pd.to_numeric(df["CantBombeableCC"])
                    df["CantRemanente"] = pd.to_numeric(df["CantRemanente"])
                    df["CantBloqueada"] = pd.to_numeric(df["CantBloqueada"])
                elif vTag == "COMPARAINVENTARIO":
                    df["InventarioBCN"] = pd.to_numeric(df["InventarioBCN"])
                    df["InventarioECC"] = pd.to_numeric(df["InventarioECC"])
                    df["InventarioS4H"] = pd.to_numeric(df["InventarioS4H"])
                elif (vTag[:11] == "MOVIMIENTOS" or vTag[:6] == "FLUJOS") and ItpInfo != "InfBalance":
                    df["vlCantVolFuente"] = pd.to_numeric(df["vlCantVolFuente"])
                    df["vlCantVolReconciliado"] = pd.to_numeric(df["vlCantVolReconciliado"])
                    df["vlCantVolConciliado"] = pd.to_numeric(df["vlCantVolConciliado"])
                    df["vlCantMasFuente"] = pd.to_numeric(df["vlCantMasFuente"])
                    df["vlCantMasReconciliado"] = pd.to_numeric(df["vlCantMasReconciliado"])
                    df["vlCantMasConciliado"] = pd.to_numeric(df["vlCantMasConciliado"])                
                elif vTag[:11] == "MOVIMIENTOS" or vTag[:3] == "LOG":                    
                    df["vlContable"] = pd.to_numeric(df["vlContable"])                                    
                elif vTag[:7] == "BALANCE":
                    if ItpInfo == "InfBalance":
                        df["InvIni"] = pd.to_numeric(df["InvIni"])
                        df["vlEntradas"] = pd.to_numeric(df["vlEntradas"])
                        df["vlSalidas"] = pd.to_numeric(df["vlSalidas"])
                        df["InvFin"] = pd.to_numeric(df["InvFin"])
                        df["vlDesbalance"] = pd.to_numeric(df["vlDesbalance"])
                    else:
                        df["InvIniVol"] = pd.to_numeric(df["InvIniVol"])
                        df["vlEntVol"] = pd.to_numeric(df["vlEntVol"])
                        df["vlSalVol"] = pd.to_numeric(df["vlSalVol"])
                        df["InvFinVol"] = pd.to_numeric(df["InvFinVol"])
                        df["vlDesbalanceVol"] = pd.to_numeric(df["vlDesbalanceVol"])
                        df["InvIniMas"] = pd.to_numeric(df["InvIniMas"])
                        df["vlEntMas"] = pd.to_numeric(df["vlEntMas"])
                        df["vlSalMas"] = pd.to_numeric(df["vlSalMas"])
                        df["InvFinMas"] = pd.to_numeric(df["InvFinMas"])
                        df["vlDesbalanceMas"] = pd.to_numeric(df["vlDesbalanceMas"])
                    
                writer = pd.ExcelWriter(InmExcel)
                df.to_excel(writer, index=False)            
                writer.close()            
                vArchivo = ItxRutaTrabajo + "/" + InmExcel
                os.startfile(vArchivo)
                vMsg = "Se proceso adecuadamente " + str(I) + " archivos XML's"
            except PermissionError:
                messagebox.showinfo(message="Error: archivo Excel:" + InmExcel + " esta abierto.", title= vFuente +" .::AppBCN")
        else:
            messagebox.showinfo(message="No existe información: \n" + vTag, title= vFuente + " .::AppBCN")

def getConvertirXML(IDatosRS, InmTag):    
    # print (IDatosRS)
    vCont = 0
    vEntrar = True     
    arrItems = []
    if len(IDatosRS) > 0:        
        dbColQuery = IDatosRS[0].keys()
        for I in range(len(IDatosRS)):
            vCont = vCont + 1
            if vEntrar == True:
                vXMLs = "<" + InmTag + ">"
                vEntrar = False                    
            vRegXML = "<reg>"
            for itm in dbColQuery:
                vRegXML += "<" + itm + ">" + str(IDatosRS[I][itm]) + "</" + itm + ">"    
            vRegXML += "</reg>"             
            vXMLs = vXMLs + vRegXML        
            if vCont == 90:                        
                vXMLs += "</" + InmTag + ">"
                arrItems.append(vXMLs)       
                vCont = 0
                vEntrar = True
            
        if vEntrar == False:
            vXMLs += "</" + InmTag + ">"
            arrItems.append(vXMLs)
    return arrItems

def WSSAPECCECP(ItpInfo, IarrParametros, IFechaFin):
    headers = {'Content-Type': 'application/json; charset=utf-8'}
    vtimestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")       
    varrData = '{[]}'
    vFechaAuxF = IFechaFin[:10]
    vUsr = IarrParametros["InfoURLSAPECCECP"]["idUsr"]
    vPwd = IarrParametros["InfoURLSAPECCECP"]["pwUsr"]
        
    if ItpInfo == "INVENTARIO":
        vTransaccionOrigen = "Comparativo de Inventarios"
        vEndPoint = IarrParametros["InfoURLSAPECCECP"]["txURL"] + IarrParametros["InfoURLSAPECCECP"]["txMetodoInventario"]
        arrCenLog = IarrParametros["InfoURLSAPECCECP"]["arrCenLog"]
        arrAlmLog = IarrParametros["InfoURLSAPECCECP"]["arrAlmLog"]
        vPayLoad = {"FechaConsulta": str(vFechaAuxF),
                    "SistemaOrigen": "ARESGRB",
                    "CentroLogistico": arrCenLog,
                    "Almacen": arrAlmLog
                    } 
    else:
        vTransaccionOrigen = "Comparativo de Costos"
        vEndPoint = IarrParametros["InfoURLSAPECCECP"]["txURL"] + IarrParametros["InfoURLSAPECCECP"]["txMetodoCosto"]
        vPayLoad = {"SistemaOrigen": "AORA",            
                    "Periodo": str(IFechaFin[6:7]),
                    "Anio": str(IFechaFin[:4]),
                    "TipoValor": 4,
                    "ObjetoCostos": [{"Nombre": "RF5050"}]
                    }
    # print (vPayLoad)
    try:
        # payload = f"""<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:int="http://COMMON/Interfaces"><soapenv:Header/><soapenv:Body><int:MT_RespuestaLegado>{ItxXML}</int:MT_RespuestaLegado></soapenv:Body></soapenv:Envelope>"""                    
        response = requests.post(vEndPoint, headers=headers, data = json.dumps(vPayLoad), auth = HTTPBasicAuth(vUsr, vPwd))
        vCode = response.status_code
        # print("Envio ", IPayLoad)
        if vCode == 200:
            vMsg = vtimestamp + " INFO: Envio exitoso del Registro " + vTransaccionOrigen + " (estado: 200)!!!\n"            
            varrDataAux = json.loads(response.text)     
            # print(varrDataAux)                   
            if ItpInfo == "INVENTARIO":
                varrData = varrDataAux["INVENTARIO"]
            else:
                 varrData = varrDataAux["CECO"]                        
        else:            
            vMsg = vtimestamp + " ERROR: Estado: " +str(vCode) + " - " + vTransaccionOrigen + " \n"
            
    except requests.ConnectionError:
        vMsg = vtimestamp + " ERROR: Estado: 400 - No se pudo conectar con el servidor. Asegurese de que el servidor este en funcionamiento.\n"
        vCode = 400
    except requests.Timeout:
        vMsg = vtimestamp + " ERROR: Estado: 500 - Se agotó el tiempo de espera de la solicitud. Verifique su conexión a Internet o vuelva a intentarlo más tarde.\n"
        vCode = 500
    except requests.exceptions.HTTPErrorn as e:
        vMsg = vtimestamp + " ERROR: Se produjo el siguiente : " + e + "\n"
        vCode = 0
    return vCode, vMsg, varrData
                    
def getIntegrarInfo(ItpInfo, IarrParametros, IUsrAuditoria):
    vdtFormato = "%Y-%m-%d %H:%M:%S"
    vFuente = "GRB"
    dtFechaIni = txtFechaIni.get() + " 00:00:00"
    dtFechaFin = txtFechaFin.get() + " 23:59:59"
    # print(ItpInfo)
    vEntrar = True
    vRespuesta = False
    vSqlAux = ""
    # Reglas para Información Operativa 
    if ItpInfo == "InfOPerativo":
        vDatosDB = IarrParametros["InfoDBAORA"]
        vIdOpcion =  cmbIntegrarInfo.get()
        if vIdOpcion[:2] == "01":
            vTagQuery = "INVOPERAORA"    
            vRespuesta = messagebox.askyesnocancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\n Opciones: \n * Si: Fecha de Inventario Inicial: "+ str(dtFechaIni) + "\n * No: Fecha de Inventario Final :"+ str(dtFechaFin) + "\n * Cancelar: Cancelar tarea", title= vFuente + " .::AppBCN")                    
            if vRespuesta == True:
                vFechaAux = datetime.strptime(dtFechaIni, vdtFormato) + timedelta(minutes = -1)
            elif vRespuesta == False:
                vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds =-59)            
            elif vRespuesta is None:
                vEntrar = False           
            if vEntrar == True:
                vSql = IarrParametros["xQuerys"].find("qry" + vTagQuery)             
                vSqlAux = vSql.text.strip() 
                vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(vFechaAux))
                # print(vSqlAux)
                vMsg, vDatosRS = oConectarDB(vDatosDB, vSqlAux)
                vCantRegistros = len(vDatosRS)
                if len(vDatosRS):
                    varrItems = getConvertirXML(vDatosRS, "Inventarios")                                                                      
                else:
                    vEntrar = False
        elif vIdOpcion[:2] in ("02", "03"):        
            vTagQuery = "MOVOPERAORA"
            vTag = "Movimientos"
            if vIdOpcion[:2] == "03":
                vTagQuery = "FLUOPERAORA"
                vTag = "Flujos"
                
            vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\n Periodo Fecha Inicio: "+ str(dtFechaIni) + " hasta: "+ str(dtFechaFin) + " de ejecucion del  proceso", title = vFuente + " .::AppBCN")                                
            # print(vRespuesta)
            if vRespuesta == True:
                vSql = IarrParametros["xQuerys"].find("qry" + vTagQuery)             
                vSqlAux = vSql.text.strip() 
                vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(datetime.strptime(dtFechaIni, vdtFormato)))
                vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(datetime.strptime(dtFechaFin, vdtFormato)))            
                vMsg, vDatosRS = oConectarDB(vDatosDB, vSqlAux)
                vCantRegistros = len(vDatosRS)
                if len(vDatosRS):                  
                    varrItems = getConvertirXML(vDatosRS, vTag)
                else:
                    vEntrar = False
            else:
                vEntrar = False
        elif vIdOpcion[:2] == "04":
            vTagQuery = "INVOPERROMSS"
            vTag = "Inventarios"
            vDatosDB = IarrParametros["InfoDBROMSS"]
            vFechaAux = datetime.strptime(dtFechaIni, vdtFormato) 
            vRespuesta = messagebox.askyesnocancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\n Opciones: \n * Si: Fecha de Inventario Inicial: "+ str(dtFechaIni) + "\n * No: Fecha de Inventario Final :"+ str(dtFechaFin) + "\n * Cancelar: Cancelar tarea", title=vFuente + " .::AppBCN")
            
            if vRespuesta == False:
                vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds = 1)
            elif vRespuesta is None:
                vEntrar = False
                
            if vEntrar == True:
                vSql = IarrParametros["xQuerys"].find("qry" + vTagQuery)             
                vSqlAux = vSql.text.strip() 
                vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(vFechaAux))
                vMsg, vDatosRS = oConectarDB(vDatosDB, vSqlAux)
                vCantRegistros = len(vDatosRS)
                if len(vDatosRS):
                    vEntrar = True
                    varrItems = getConvertirXML(vDatosRS, vTag) 
                else:
                    vEntrar = False
            
        elif vIdOpcion[:2] in ("05", "07"):
            vTagQuery = "MOVOPERROMSS"
            vTag = "Movimientos"
            if vIdOpcion[:2] == "07":                
                vTagQuery = "MOVHPIARES"
                vTag = "HPI"
            else:
                vDatosDB = IarrParametros["InfoDBROMSS"]
            
            vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds = 1)
            vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea migrar información del sistema " + vIdOpcion[3:] + " ? \n\n Periodo Fecha Inicio: "+ str(dtFechaIni) + " hasta: "+ str(vFechaAux) + " de ejecucion del  proceso", title = vFuente + " .::AppBCN")
            if vRespuesta == True:
                vSql = IarrParametros["xQuerys"].find("qry" + vTagQuery)             
                vSqlAux = vSql.text.strip() 
                vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(datetime.strptime(dtFechaIni, vdtFormato)))
                vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(vFechaAux)) 
                # print("Algo ", vSqlAux, "\n")    
                vMsg, vDatosRS = oConectarDB(vDatosDB, vSqlAux)
                vCantRegistros = len(vDatosRS)                
                if len(vDatosRS):
                    varrItems = getConvertirXML(vDatosRS, vTag)            
                else:
                    vEntrar = False
            else:
                vEntrar = False                
        else: 
            vEntrar = False
            messagebox.showinfo(message="Esta opción no esta disponible para ejecucion:" + vIdOpcion, title = vFuente + " .::AppBCN")
    elif ItpInfo == "InfConsolidado":
        # Opciones de Consolidacion
        vSqlAux = ""
        vIdOpcion =  cmbConsolidarInfo.get()
        vCantRegistros = 1
        if vIdOpcion[:2] == "01":
            vTagQuery = "INVCONSBCN"
            vTag = "Inventarios"

            vRespuesta = messagebox.askyesnocancel(message="¿Esta seguro que desea ejecutar la información del sistema " + vIdOpcion[3:] + " ? \n\n Opciones: \n * Si: Fecha de Inventario Inicial: "+ str(dtFechaIni) + "\n * No: Fecha de Inventario Final :"+ str(dtFechaFin) + "\n * Cancelar: Cancelar tarea", title=vFuente + " .::AppBCN")
            if vRespuesta == True:
                vFechaAux = datetime.strptime(dtFechaIni, vdtFormato) + timedelta(minutes = -1)
            elif vRespuesta == False:
                vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds =-59)            
            elif vRespuesta is None:
                vEntrar = False
                                        
            if vEntrar == True:                            
                vDatosRS = [{'dtInventario': vFechaAux}]
                varrItems = getConvertirXML(vDatosRS, vTag)            
                vEntrar = True         
        elif vIdOpcion[:2] in ("02", "07", "08", "09"):        
            vTagQuery = "MOVCONSBCN"
            vTag = "Movimientos"
            if vIdOpcion[:2] == "07": # Opc Balance de Signo Contrario
                vTagQuery = "RNSIGCONTRARIO"
                vTag = "Movimientos"                
            elif vIdOpcion[:2] == "09": # Opc Dif Balance
                vTagQuery = "DIFBALANCE"
                vTag = "Movimientos"
            vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea procesar información del sistema " + vIdOpcion[3:] + " ? \n\n Periodo Fecha Inicio: "+ str(dtFechaIni) + " hasta: "+ str(dtFechaFin) + " de ejecucion del  proceso", title = vFuente + " .::AppBCN")
            if vRespuesta == True:                  
                if vIdOpcion[:2] == "08": # Opc Regla de Balance
                    vTagQuery = "REGBALANCE"
                    vTag = "Movimientos"        
                    vDatosDB = IarrParametros["InfoDBBCN"]                                                                                    
                    vSql = IarrParametros["xQuerys"].find("qry" + vTagQuery)             
                    vSqlAux = vSql.text.strip() 
                    vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(dtFechaIni))
                    vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(dtFechaFin)) 
                    # print("Algo ", vSqlAux, "\n")    
                    vMsg, vDatosRS = oConectarDB(vDatosDB, vSqlAux)
                    vCantRegistros = len(vDatosRS)                
                    if len(vDatosRS):
                        varrItems = getConvertirXML(vDatosRS, vTag) 
                    else:
                        vEntrar = False        
                else:
                    vDatosRS = [{'dtMovimientoIni': dtFechaIni, 'dtMovimientoFin': dtFechaFin}]
                    varrItems = getConvertirXML(vDatosRS, vTag)            
            else:
                vEntrar = False              
        else:
            vEntrar = False
            messagebox.showinfo(message="Esta opción no esta disponible para ejecucion:\n\n" + vIdOpcion, title = vFuente + " .::AppBCN")
    
    elif ItpInfo == "InfBalance":
        # Opciones de Balance Logistico
        vSqlAux = ""        
        vIdOpcion =  cmbBalanceInfo.get()
        vCantRegistros = 1
        if vIdOpcion[:2] == "01":
            vTagQuery = "TRANSMOVCONSBALLOG"
            vTag = "MovimientosLogistico"

            vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea transformar información consolidada a logistica  " + vIdOpcion[3:] + " ? \n\n Periodo Fecha Inicio: "+ str(dtFechaIni) + " hasta: "+ str(dtFechaFin) + " de ejecucion del  proceso", title = vFuente + " .::AppBCN")
            # print ("algo ", vRespuesta, " ", vEntrar)
            if vRespuesta == True:
                vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds = 1)    
                vDatosRS = [{'dtMovimientoIni': dtFechaIni, 'dtMovimientoFin': vFechaAux}]
                varrItems = getConvertirXML(vDatosRS, vTag)                         
            else:
                vEntrar = False
        elif vIdOpcion[:2] == "02":
            vTagQuery = "TRANSMOVCONSCOSTO"
            vTag = "MovimientosCosto"
            vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea transformar información consolidada a Costos  " + vIdOpcion[3:] + " ? \n\n Periodo Fecha Inicio: "+ str(dtFechaIni) + " hasta: "+ str(dtFechaFin) + " de ejecucion del  proceso", title = vFuente + " .::AppBCN")
            if vRespuesta == True:                
                vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) + timedelta(seconds = 1)    
                vDatosRS = [{'dtMovimientoIni': dtFechaIni, 'dtMovimientoFin': vFechaAux}]
                varrItems = getConvertirXML(vDatosRS, vTag)            
            else:
                vEntrar = False                
        else:
            vEntrar = False
            messagebox.showinfo(message="Esta opción no esta disponible para ejecucion:" + vIdOpcion, title = vFuente + " .::AppBCN")
    elif ItpInfo == "InfEnviosWS":  
        vSqlAux = ""        
        vIdOpcion =  cmbEnvioWSARESInfo.get()    
        vLstBalance = ['INVENTARIOS', 'MOVIMIENTOS', 'COSTOS']                                  
        if vIdOpcion[:2] in ("04", "05"):
            vEntrar = False
            vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea actualizar la información enviada a SAP ? \n\n Fecha de Contabilizacion: "+ str(dtFechaIni), title = vFuente + " .::AppBCN")
            if vRespuesta == True:
                vTag = "Movimientos"
                
                vTagQuery = "MOVLOGISTICOOK"
                if vIdOpcion[:2] == "05":                    
                    vTagQuery = "MOVCOSTOOK"
                
                vDatosDB = IarrParametros["InfoDBAORAQAS"] # Ojo Cambiar a PRD 
                vSql = IarrParametros["xQuerys"].find("qry" + vTagQuery)             
                vSqlAux = vSql.text.strip() 
                
                vFechaAux = datetime.strptime(dtFechaIni, vdtFormato) 
                vSqlAux = vSqlAux.replace('[dtConsultaIni]', str(vFechaAux))
                
                vFechaAux = datetime.strptime(dtFechaFin, vdtFormato) 
                vSqlAux = vSqlAux.replace('[dtConsultaFin]', str(vFechaAux))
                
                vMsg, vDatosRS = oConectarDB(vDatosDB, vSqlAux)
                vCantRegistros = len(vDatosRS)
                if len(vDatosRS):
                    vEntrar = True
                    varrItems = getConvertirXML(vDatosRS, vTag)                     
                else:
                    vEntrar = False
        elif vIdOpcion[:2] in ("06", "07"):            
            vEntrar = False                        
            vRespuesta = messagebox.askokcancel(message="¿Esta seguro que desea actualizar la información enviada a SAP ? \n\n Fecha de Contabilizacion: "+ str(dtFechaFin), title = vFuente + " .::AppBCN")
            if vRespuesta == True:
                vTag = "INVENTARIO"
                vTagQuery = "INVENTARIOSAPECC"
                if vIdOpcion[:2] == "07":
                    vTagQuery = "COSTOSAP"
                    vTag = "CECO"
                vCode, vMsg, vDatosRS = WSSAPECCECP(vTag, varrParametros, dtFechaFin)
                vCantRegistros = len(vDatosRS)                
                if len(vDatosRS) > 0 and vCode == 200:
                    vEntrar = True
                    varrItems = getConvertirXML(vDatosRS, vTag)
                else: 
                    EscribirLog(vMsg)            
        else:            
            ItpInfo = vLstBalance[int(vIdOpcion[:2])-1]                    
            EnvioARES(ItpInfo, IarrParametros, IUsrAuditoria)
            vEntrar = False
            
        # print(vDatosRS)
    # print(vEntrar, " ", vSqlAux)
    # print(vEntrar, " ", vSqlAux, " ", vTagQuery, " ", varrItems) 
    if vEntrar == True:        
        vSqlAux = vSqlAux + "\n"
        EscribirLog(vSqlAux)
        IDatosDB = IarrParametros["InfoDBBCN"]
        oConnet = pymssql.connect(server = IDatosDB["ServidorDB"], port = IDatosDB["PuertoDB"], user = IDatosDB["UsrDB"], password = IDatosDB["PwdDB"], database = IDatosDB["BaseDatos"], as_dict=True, tds_version="7.0", login_timeout = 5)
        rsConsulta = oConnet.cursor()            
        for Fil in range(len(varrItems)):
            # print(IUsrAuditoria, " ", vFuente, " ",  vTagQuery )
            # print(varrItems[Fil] ) 
            # vEntrar = False
            vtimestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            vMsg = vtimestamp + " Info: Fuente: " + vFuente + " Tag: " + vTagQuery + " Usuario: " + IUsrAuditoria + " No. Registros: " + str(vCantRegistros) + " Paquete XML: " + str(Fil+1) + " de " + str(len(varrItems)) + "\n"
            EscribirLog(vMsg)
            vMsg = varrItems[Fil] + "\n\n"
            EscribirLog(vMsg)
            if vEntrar == True:        
                try:
                    vSqlGen = """EXEC BCN.spAppIntegrarProcesarInfo '""" + IUsrAuditoria + """', '""" + vFuente + """', '""" + vTagQuery + """', '""" + varrItems[Fil] + """';"""
                    # print(vSqlGen)
                    rsConsulta.execute(vSqlGen)
                    oConnet.commit()
                    vEntrar = True
                except Exception as ex:
                    vEntrar = False
                    messagebox.showerror(message="Error: \n" + str(ex), title="AppBCN")
                
        oConnet.close()            
        if vEntrar == True: 
            messagebox.showinfo(message="Se cargo la información: \n\n" + vIdOpcion + " "+ str(dtFechaIni)+ "\n\nNumero de Registros: " + str(vCantRegistros), title="AppBCN")
    else:        
        # print(vSqlAux) 
        if len(vSqlAux) > 0:
            EscribirLog(vSqlAux)
        if vRespuesta == True:
            messagebox.showinfo(message="No existe información para la acción: \n\n" + vIdOpcion, title = vFuente + " .::AppBCN")
    # print(" ss", vIdOpcion, " ", dtFechaIni)


# Inicio de la app Movimientos Logisticos GRC
if __name__ == '__main__':
    try:
        # locale.setlocale(locale.LC_ALL, 'es_ES.UTF-8')  # Para Linux/Mac
        # locale.setlocale(locale.LC_ALL, 'Spanish_Spain.1252')  es_CO
        locale.setlocale(locale.LC_ALL, 'es_CO.UTF-8')
    except locale.Error:
        print("Configuración regional no soportada. Usa una predeterminada.")
        locale.setlocale(locale.LC_ALL, '')


    vUsrAuditoria = "AdminBCN"
    # vdtContableIni = datetime.now().strftime("%Y-%m-") + "01 00:00:00"
    # vdtContableFin = datetime.now().strftime("%Y-%m-") + "01 23:59:59"    
    vdtContableIni = datetime.now().strftime("%Y-%m-") + "01"
    vdtContableFin = datetime.now().strftime("%Y-%m-") + "01"
    # vdtContableIni = datetime.now().strftime("%Y-") + "07-01"
    # vdtContableFin = datetime.now().strftime("%Y-") + "07-01"
        
    vArcXML = "Conf_BCN.xml"
    # Estructura para la gestion de los XML's 
    varrMoverXML = []
    varrProcesarXML = []

    # Parametro de inicio 
    PosX = 5
    PosY = 10
    vRutaTrabajo = os.getcwd().replace(os.path.sep, "/")
    vMsg, vboLeerRuta, varrParametros = CargarConfiguracionXML(vRutaTrabajo, vArcXML)


    # getInfoAORA(varrParametros, 'INVOPERAORA', vdtContableIni, vdtContableFin, vUsrAuditoria)

    # getInvAORA(varrParametros, vdtContableIni, vUsrAuditoria)
    # getMovAORA(varrParametros, vFecha, vUsrAuditoria)
    # getRecBIC(varrParametros, vFecha)
    if vboLeerRuta == True:
        
        # Crea la interface de usuario 
        ventanaWin = tk.Tk()
        ventanaWin.config(width=590, height=235)
        ventanaWin.title("App Informacion Balance GRB")
        try:
            icoApp = tk.PhotoImage(file = "icon-16.png")    
            ventanaWin.iconphoto(False, icoApp)
        except:
            vMsg = "Error en icono"
        
        # Primera opción:
        etiqueta01 = tk.Label(ventanaWin, text="Seleccione fecha de inicio y fin del periodo Contable:")
        etiqueta01.place(x = PosX, y = PosY-5)

        etiqueta02 = tk.Label(ventanaWin, text="Fecha desde :")
        etiqueta02.place(x = PosX, y = PosY + 14)
        
        txtFechaIni = ttk.Entry( font = font.Font(family="Times", size=11), width=10)
        txtFechaIni.insert(0, vdtContableIni)
        # txtRutaArcXML.config (state = "readonly")
        txtFechaIni.place(x = PosX + 110, y = PosY+15)            

        etiqueta03 = tk.Label(ventanaWin, text="hasta :")
        etiqueta03.place(x = PosX + 221, y = PosY + 14)
        
        txtFechaFin = ttk.Entry( font = font.Font(family="Times", size=11), width=10)
        txtFechaFin.insert(0, vdtContableFin)
        # txtRutaArcXML.config (state = "readonly")
        txtFechaFin.place(x = PosX + 265, y = PosY+14)

        etiqueta04 = tk.Label(ventanaWin, text="Integrar Información:")
        etiqueta04.place(x = PosX, y = PosY + 40)
        
        # vLstOpciones = ["01. AORA: Inventario Inicial Operativo", "02. AORA: Inventario Final Operativo", "03. AORA: Movimientos Operativos", "04. AORA: Flujos Operativos", "05. ROMSS: Inventario Inicial Operativo", "06. ROMSS: Inventario Final Operativo", "07. ROMSS: Movimientos Operativo", "08. BCN: Foto Inventario"]
        vLstOpciones = ["01. AORA: Inventario Operativo",  "02. AORA: Movimientos Operativos", "03. AORA: Flujos Operativos", "04. ROMSS: Inventario Operativo", "05. ROMSS: Movimientos Operativo", "06. BCN: Foto Inventario", "07. ARES: Movimientos HPI", "08. BCN: Balance Operativo"]
        cmbIntegrarInfo = ttk.Combobox(values=vLstOpciones, width= 34, state='readonly')
        cmbIntegrarInfo.current(0)
        cmbIntegrarInfo.place(x = PosX + 117, y = PosY + 42)

        botonIntegrarInfo = ttk.Button(text="Ejecutar", command = lambda: getIntegrarInfo("InfOPerativo", varrParametros, vUsrAuditoria))
        botonIntegrarInfo.place(x = PosX + 344, y = PosY + 40)

        botonIntegrarInfoHTML = ttk.Button(text="Rev. HTML", command = lambda: getVisualizarInfoHTML("InfOPerativo", varrParametros, vRutaTrabajo))
        botonIntegrarInfoHTML.place(x = PosX + 420, y = PosY + 40)

        botonIntegrarInfoExcel = ttk.Button(text="Rev. Excel", command = lambda: getVisualizarInfoExcel("InfOPerativo", varrParametros, vRutaTrabajo))
        botonIntegrarInfoExcel.place(x = PosX + 495, y = PosY + 40)

        etiqueta05 = tk.Label(ventanaWin, text="Consolidar Información:")
        etiqueta05.place(x = PosX, y = PosY + 66)        
        
        # vLstOpciones = ["01. BCN: Inventario Inicial", "02. BCN: Inventario Final", "03. BCN: Movimientos", "04. BCN: Balance ALMACEN", "05. BCN: Balance POOL", "06. BCN: Balance UNIDAD DE PROCESO"]
        vLstOpciones = ["01. BCN: Inventarios", "02. BCN: Movimientos", "03. BCN: Balance ALMACEN", "04. BCN: Balance POOL", "05. BCN: Balance UNIDAD DE PROCESO", "06. BCN: Foto Inventario", "07. BCN: Corregir  Bal. Sig. Contrario", "08. BCN: Aplicar Regla de Balance", "09. BCN: Diferencia Balance"]
        cmbConsolidarInfo = ttk.Combobox(values=vLstOpciones, width= 31, state='readonly')
        cmbConsolidarInfo.current(0)
        cmbConsolidarInfo.place(x = PosX + 135, y = PosY + 68)
        
        botonConsolidarInfo = ttk.Button(text="Ejecutar", command = lambda: getIntegrarInfo("InfConsolidado", varrParametros, vUsrAuditoria))
        botonConsolidarInfo.place(x = PosX + 344, y = PosY + 66)

        botonConsolidarInfoHTML = ttk.Button(text="Rev. HTML", command = lambda: getVisualizarInfoHTML("InfConsolidado", varrParametros, vRutaTrabajo))
        botonConsolidarInfoHTML.place(x = PosX + 420, y = PosY + 66)

        botonConsolidarInfoExcel = ttk.Button(text="Rev. Excel", command = lambda: getVisualizarInfoExcel("InfConsolidado", varrParametros, vRutaTrabajo))
        botonConsolidarInfoExcel.place(x = PosX + 495, y = PosY + 66)

        etiqueta06 = tk.Label(ventanaWin, text="Transformación Logistica:")
        etiqueta06.place(x = PosX, y = PosY + 92)
        
        # BCN: Costos - Colectores", "05. BCN: Costos - Pooles", "06. BCN: Costos - Plantas", "04. BCN: Foto Inventario"
        vLstOpciones = ["01. BCN: Movimientos Logisticos", "02. BCN: Movimientos de Costos", "03. BCN: Balance GRB CeLo: 2000", "04. BCN: Balance Reexpido CeLo: 3501", "05. BCN: Balance Impala CeLo: 4130"]
        cmbBalanceInfo = ttk.Combobox(values=vLstOpciones, width= 31, state='readonly')
        cmbBalanceInfo.current(0)
        cmbBalanceInfo.place(x = PosX + 135, y = PosY + 94)
        
        botonBalanceInfo = ttk.Button(text="Ejecutar", command = lambda: getIntegrarInfo("InfBalance", varrParametros, vUsrAuditoria))
        botonBalanceInfo.place(x = PosX + 344, y = PosY + 92)

        botonBalanceInfoHTML = ttk.Button(text="Rev. HTML", command = lambda: getVisualizarInfoHTML("InfBalance", varrParametros, vRutaTrabajo))
        botonBalanceInfoHTML.place(x = PosX + 420, y = PosY + 92)

        botonBalanceInfoExcel = ttk.Button(text="Rev. Excel", command = lambda: getVisualizarInfoExcel("InfBalance", varrParametros, vRutaTrabajo))
        botonBalanceInfoExcel.place(x = PosX + 495, y = PosY + 92)

        # Envio de Informacion WebServices
        etiqueta07 = tk.Label(ventanaWin, text="Envío BCN a WS:")
        etiqueta07.place(x = PosX, y = PosY + 118)
        
        vLstOpciones = ["01. BCN: Inventario Logistico", "02. BCN: Movimiento Logistico", "03. BCN: Movimiento de Costos", "04. ARES: Rev. Procesamiento Logistico", "05. ARES: Rev. Procesamiento Costo", "06. BCN: Comparativo Inventario", "07. BCN: Comparativo Costos" ]
        cmbEnvioWSARESInfo = ttk.Combobox(values=vLstOpciones, width= 35, state='readonly')
        cmbEnvioWSARESInfo.current(0)
        cmbEnvioWSARESInfo.place(x = PosX + 110, y = PosY + 120) 

        # botonEnvioWSARESInfo = ttk.Button(text="Ejecutar", command = lambda: EnvioARES("EnvioBCN_ARES", varrParametros, vUsrAuditoria))
        botonEnvioWSARESInfo = ttk.Button(text="Ejecutar", command = lambda: getIntegrarInfo("InfEnviosWS", varrParametros, vUsrAuditoria))
        botonEnvioWSARESInfo.place(x = PosX + 344, y = PosY + 118)

        # botonEnvioWSARESInfoHTML = ttk.Button(text="Rev. HTML", command = lambda: getVisualizarInfoHTML("InfBalance", varrParametros, vRutaTrabajo))
        # botonEnvioWSARESInfoHTML.place(x = PosX + 420, y = PosY + 118)

        botonEnvioWSARESInfoExcel = ttk.Button(text="Rev. Excel", command = lambda: getVisualizarInfoExcel("InfEnviosWS", varrParametros, vRutaTrabajo))
        botonEnvioWSARESInfoExcel.place(x = PosX + 495, y = PosY + 118)
                
        # botonBalNodo = ttk.Button(text="Balance de Nodos      ", command = lambda: getBalNodos(varrParametros, vRutaTrabajo))
        # combo.bind("<<ComboboxSelected>>", selection_changed)
        # botonBalNodo.place(x = PosX + 290, y = PosY + 40)
        
        ventanaWin.mainloop()