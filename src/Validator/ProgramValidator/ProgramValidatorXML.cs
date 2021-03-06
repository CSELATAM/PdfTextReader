﻿using PdfTextReader.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PdfTextReader
{
    class ProgramValidatorXMLStats
    {
        public int errors;
        public int total;
        public string text;
    }

    public class ProgramValidatorXML
    {
        static ProgramValidatorXMLStats GlobalStats = new ProgramValidatorXMLStats();
        bool bodyConditions = false;
        bool titleConditions = false;
        //bool hierarchyConditions = false;
        bool numeroDaPaginaConditions = false;
        //bool anexoConditions = true;
        bool roleConditions = true;
        bool signConditions = true;
        bool tipoArtigoConditions = true;
        int DocumentsCount = 0;
        int DocumentsCountWithError = 0;
        string logDir;
        string XMLErrorsDir;
        string currentName;

        public void ValidateArticle(string folder)
        {
            currentName =  VirtualFS.GetDirectoryName(folder);

            logDir = VirtualFS.GetDirectoryCreateDirectory($"{folder}/Log");
            XMLErrorsDir = VirtualFS.GetDirectoryCreateDirectory($"{folder}/XML-Errors");
            folder = folder + "/XMLs";
            
            foreach (var file in VirtualFS.DirectoryInfoEnumerateFiles(folder, "*.xml"))
            {
                DocumentsCount++;
                Validate(file);
            }
            CalculatePrecision(DocumentsCount, DocumentsCountWithError);
        }

        void CalculatePrecision(int docs, int error)
        {
            GlobalStats.errors += error;
            GlobalStats.total += docs;

            float result = (1.0f - ((float) error /(float) docs)) * 100;
            string text = $"Article precision: {result.ToString("00.00")}%  \nArticles processed: {docs}  \nArticles With Error: {error}";
            VirtualFS.FileWriteAllText($"{logDir}/ArticlePrecision.txt", text);

            GlobalStats.text += $" \n\n{currentName} - {text}";
        }

        public static float CreateFinalStats(string filename)
        {
            int error = GlobalStats.errors;
            int docs = GlobalStats.total;

            float result = (1.0f - ((float)error / (float)docs)) * 100;
            string text = $"Article precision: {result.ToString("00.00")}%  \nArticles processed: {docs}  \nArticles With Error: {error} \n\n{GlobalStats.text}";
            VirtualFS.FileWriteAllText(filename, text);

            return result;
        }

        void Validate(VirtualFS.VFileInfo file)
        {
            //Caso o proximo artigo venha sem assinatura ou cargo, ele não é erro.
            roleConditions = true;
            signConditions = true;
            tipoArtigoConditions = true;

            XDocument doc = VirtualFS.XDocumentLoad(file.FullName);
            //Elementos Metadado e Conteudo
            foreach (XElement el in doc.Root.Elements())
            {
                foreach (XAttribute item in el.Attributes())
                {
                    if (item.Name == "NumPagina")
                        CheckNumeroDaPagina(item.Value);
                }

                foreach (XElement item in el.Elements())
                {
                    if (item.Name == "Titulo")
                        CheckTitle(item.Value);

                    if (item.Name == "Corpo")
                        CheckBody(item.Value);

                    if (item.Name == "Cargo")
                        CheckRoles(item.Value);

                    if (item.Name == "Autores")
                    {
                        foreach (var i in item.Elements())
                        {
                            foreach (XAttribute at in i.Attributes())
                            {
                                CheckRoles(at.Value);
                            }

                            CheckSigns(i.Value);
                        }

                    }

                    if (item.Name == "Anexo")
                    {
                        foreach (var anexo in item.Elements())
                        {
                            //CheckAnexo();
                        }
                    }
                }
            }


            if (!bodyConditions 
                || !titleConditions 
                || !roleConditions 
                || !signConditions 
                || !tipoArtigoConditions
                || !numeroDaPaginaConditions)
            {

                StringBuilder error = new StringBuilder();


                if (!bodyConditions)
                {
                    error.Append("Body-");
                }

                if (!titleConditions)
                {
                    error.Append("Title-");
                }

                if (!roleConditions)
                {
                    error.Append("Role-");
                }

                if (!signConditions)
                {
                    error.Append("Sign-");
                }

                if (!numeroDaPaginaConditions)
                {
                    error.Append("NumPag-");
                }

                if (!tipoArtigoConditions)
                {
                    error.Append("Tipo-");
                }

                DocumentsCountWithError++;
                file.CopyTo($"{XMLErrorsDir}/{file.Name.Replace(".xml", "")}-ISSUE-{error.ToString()}.xml");
            }
        }

        void CheckNumeroDaPagina(string text)
        {
            if (text != "-1")
                numeroDaPaginaConditions = true;
        }
        
        //void CheckHierarchy(string text)
        //{
        //    if (!String.IsNullOrWhiteSpace(text))
        //    {
        //        if (text.Replace("o", "O").ToUpper() == text.Replace("o", "O"))
        //            hierarchyConditions = true;
        //    }
        //}

        void CheckTitle(string text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                titleConditions = false;
                if (text.Replace("o", "O").ToUpper() == text.Replace("o", "O"))
                {
                    foreach (string t in Normas)
                    {
                        if (text.ToLower().Contains(t.ToLower()))
                            titleConditions = true;
                    }
                }
            }
        }

        void CheckBody(string text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                var bodyParts = text.Split('\n');
                if (bodyParts.Count() > 2)
                {
                    var lastItem = bodyParts[bodyParts.Count() - 1];
                    var lastButOne = bodyParts[bodyParts.Count() - 2];

                    if (lastItem.ToUpper() != lastItem || lastButOne.ToUpper() != lastButOne)
                        bodyConditions = true;
                }
                else if (bodyParts.Count() == 2)
                {
                    var lastItem = bodyParts[bodyParts.Count() - 1];

                    if (lastItem.ToUpper() != lastItem)
                        bodyConditions = true;
                }
            }

        }

        //void CheckAnexo(string text)
        //{
        //    if (String.IsNullOrWhiteSpace(text))
        //        anexoConditions = false;
        //}

        void CheckSigns(string text)
        {
            signConditions = false;
            foreach (string item in ExclusiveWords)
            {
                if (!text.ToLower().Contains(item))
                    signConditions = true;
            }
        }


        void CheckRoles(string text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                roleConditions = false;
                if (text.Replace("o", "O").ToUpper() != text.Replace("o", "O"))
                    if (text.Length < 90)
                        roleConditions = true;
            }
        }

        void CheckTipoArtigo(string text)
        {
            if (text.All(Char.IsDigit))
                tipoArtigoConditions = false;
        }

        string[] ExclusiveWords =
        {
            "substituto",
            "coordenadora",
            "coordenador",
            "procuradoria",
            "federal",
            "agência",
            "agencia",
            "civil"
        };

        string[] Normas =
        {
            "Ação Declaratória de Constitucionalidade",
            "Ação Direta de Inconstitucionalidade e Ação Declaratória de Constitucionalidade",
            "Ação penal originária",
            "Acórdão",
            "Acordão",
            "Acordo Coletivo de Trabalho",
            "Aditamento à Pauta",
            "Agenda Regulatória",
            "Agravo",
            "Agravo de Instrumento",
            "Ajuste",
            "Alvará",
            "Apelação",
            "Aresto",
            "Argüição de Descumprimento de Preceito Fundamental",
            "Arqüição de suspeição e/ou Impedimento",
            "Ata",
            "Ata de Julgamento",
            "Ata Suplementar",
            "Ato",
            "Ato Complementar",
            "Ato Concessório",
            "Ato Declaratório",
            "ATOS DECLARATÓRIOS",
            "Ato Declaratório Concessivo",
            "Ato Declaratório Conjunto",
            "Ato Declaratório Especial",
            "Ato Declaratório Executivo",
            "Ato Declaratório Interpretativo",
            "Ato Declaratório Normativo",
            "Ato Normativo",
            "Ato Ordinatório",
            "Ato Regimental",
            "Ato Regulamentar",
            "Auto de Infração",
            "Autorização",
            "autorização de acesso",
            "Aviso",
            "Aviso aos Acionistas",
            "Aviso de Adiamento",
            "Aviso de Adjudicação",
            "Aviso de Alienação",
            "Aviso de Alteração",
            "Aviso de Anulação",
            "Aviso de Audiência Pública",
            "Aviso de Cadastramento",
            "Aviso de Cancelamento",
            "Aviso de Cancelamento de Termo Aditivo",
            "Aviso de Chamamento Público",
            "Aviso de Circular",
            "Aviso de Coleta de Preços",
            "Aviso de Convalidação",
            "Aviso de Credenciamento",
            "Aviso de Dispensa de Licitação",
            "Aviso de Eleição",
            "Aviso de Extravio",
            "Aviso de Fato Relevante",
            "Aviso de Habilitação",
            "Aviso de Homologação",
            "Aviso de Homologação e Adjudicação",
            "Aviso de Índice Técnico",
            "Aviso de Inexigibilidade de Licitação",
            "Aviso de Julgamento",
            "Aviso de Licença",
            "Aviso de Licitação",
            "Aviso de Licitação Deserta",
            "Aviso de Licitação-Concorrência",
            "Aviso de Licitação-Convite",
            "Aviso de Licitação-Leilão",
            "Aviso de Licitação-Pregão",
            "Aviso de Licitação-RDC Eletrônico",
            "Aviso de Licitação-RDC Presencial",
            "Aviso de Licitação-Tomada de Preços",
            "Aviso de Nota Técnica",
            "Aviso de Padronização",
            "Aviso de Penalidade",
            "Aviso de Preços Registrados",
            "Aviso de Pré-Qualificação",
            "Aviso de Processo Seletivo",
            "Aviso de Proposta Comercial",
            "Aviso de Proposta Técnica",
            "Aviso de Prorrogação",
            "Aviso de Qualificação Técnica",
            "Aviso de Registro de Chapas",
            "Aviso de Registro de Preços",
            "Aviso de Relação de Compras",
            "Aviso de Rescisão",
            "Aviso de Retificação",
            "Aviso de Revogação",
            "Aviso de Seleção",
            "Aviso de Serviço e Compra",
            "Aviso de Suspensão",
            "Balancete",
            "Balancete Financeiro",
            "Balancete Patrimonial e Financeiro",
            "Balanço Patrimonial",
            "Balanço Social",
            "BOLETIM DO MÊS",
            "Carta Circular",
            "Carta de Lei",
            "Certidão",
            "Certificado",
            "Certificado de Cumprimento do TAC",
            "Certificado de Descumprimento do TAC",
            "Certificado de Empreendimento Implantado",
            "Circular",
            "Comunicado",
            "Conflito de competência e de atribuições",
            "Conselho de Justificação",
            "Consulta Pública",
            "Consulta Pública Conjunta",
            "Contrato de Gestão",
            "Convênio",
            "Correição Parcial",
            "Decisão",
            "Decisões",
            "DECISÕES",
            "Decisão Executiva",
            "Decisão Normativa",
            "Decisão/Despacho",
            "Declaração de Propósito",
            "Decreto",
            "Decreto de Pessoal",
            "Decreto Legislativo",
            "Decreto não numerado",
            "Decreto numerado",
            "Deliberação",
            "Deliberação Normativa",
            "Demonstração Contábil",
            "Desaforamento",
            "Despacho",
            "Despachos",
            "Despacho Interministerial",
            "Edital",
            "Edital da Justiça Gratuita (Art. 32 Portaria 268/2009-IN)",
            "Edital de Citação",
            "Edital de Concurso Público",
            "Edital de Convocação",
            "Edital de Intimação",
            "Edital de Leilão",
            "Edital de Notificação",
            "Edital de Processo Seletivo",
            "Edital de Resultado Final de 1ª Etapa de Concurso Público",
            "Edital de Resultado Final de 2ª Etapa de Concurso Público",
            "Edital de Vestibular",
            "Embargos",
            "Emenda",
            "Emenda Constitucional",
            "Emenda Estatutária",
            "Ementário",
            "Errata",
            "ERRO",
            "Erro",
            "Estatística",
            "Estatuto",
            "Exposição de Motivos",
            "Extrato",
            "Extrato da Ata",
            "Extrato de Acordo de Cooperação Técnica",
            "Extrato de Adesão",
            "Extrato de Ajuste",
            "Extrato de Apostilamento",
            "Extrato de Autorização de Fornecimento de Material",
            "Extrato de Autorização de Serviço",
            "Extrato de Autorização de Uso",
            "Extrato de Carta Reversal",
            "Extrato de Carta-contrato",
            "Extrato de Cessão",
            "Extrato de Cessão de Uso",
            "Extrato de Comodato",
            "Extrato de Compromisso",
            "Extrato de Concessão de Auxilio à Pesquisa",
            "Extrato de Concessão de Uso",
            "Extrato de Contrato",
            "Extrato de Convênio",
            "Extrato de Cooperação Mútua",
            "Extrato de Credenciamento",
            "Extrato de Denúncia",
            "Extrato de Depósito",
            "Extrato de Dispensa de Licitação",
            "Extrato de Distrato",
            "Extrato de Doação",
            "Extrato de Escritura de Compra e Venda",
            "Extrato de Escritura de Doação",
            "Extrato de Extinção (Lei nº 8.745 - contratação temporária)",
            "Extrato de Fornecimento",
            "Extrato de Inexigibilidade de Licitação",
            "Extrato de Instrumento Convocatório",
            "Extrato de Instrumentos Contratuais",
            "Extrato de Nota de Empenho",
            "Extrato de Ordem de Compra",
            "Extrato de Ordem de Execução de Serviço",
            "Extrato de Ordem de Fornecimento de Material",
            "Extrato de Parceria",
            "Extrato de Parecer Técnico",
            "Extrato de Permissão de Uso",
            "Extrato de Permuta",
            "Extrato de Prorrogação de Ofício",
            "Extrato de Protocolo de Cooperação",
            "Extrato de Protocolo de Intenção",
            "Extrato de Publicação",
            "Extrato de Recolhimento",
            "Extrato de Reconhecimento de Dívida",
            "Extrato de Registro de Preços",
            "Extrato de Relação de Compras",
            "Extrato de Rerratificação",
            "Extrato de Rescisão",
            "Extrato de Rescisão Contratual",
            "Extrato de Rescisão Parcial de Benefícios",
            "Extrato de Resilição",
            "Extrato de Sub-rogação",
            "Extrato de Termo Aditivo",
            "Extrato de Termo de Cooperação Técnica",
            "Extrato de Termo de Entrega",
            "Extrato de Termo de Execução Descentralizada",
            "Extrato de Termo de Parceria",
            "Extrato de Termo de Protocolo de Cooperação",
            "Extrato de Transferência de Posse",
            "Extrato Prévio",
            "Fato Relevante",
            "Grade Curricular",
            "Habeas-Corpus",
            "Habeas-Data",
            "Imagens",
            "Indeterminado",
            "Indice de Advogados",
            "Inquérito Policial Militar ou Representação Criminal",
            "Instrução",
            "Instrucão",
            "Instrução Normativa",
            "Instrucão Normativa",
            "Intimação",
            "Intimacão",
            "Lei",
            "Lei Complementar",
            "Lei Constitucional",
            "Lei Delegada",
            "Lei Ordinária",
            "Lista de Antiguidade",
            "Lista de Antiguidade das Autoridades Judiciárias do Distrito Federal",
            "Lista de Antiguidade de Magistrado",
            "Mandado de Segurança",
            "Manual de Orientação",
            "Medida Provisória",
            "Memorando de Entendimento",
            "Mensagem",
            "Norma Complementar",
            "Norma de Execução",
            "Norma Executiva",
            "Ofício circular",
            "Ordem de Serviço",
            "Orientação Normativa",
            "Parecer Normativo",
            "Parecer Técnico Conclusivo",
            "Pauta",
            "Pauta de Aditamento",
            "Pauta de Julgamento",
            "Pauta Especial",
            "Petição",
            "Plano de Correição",
            "Portaria",
            "Portaria Conjunta",
            "Portaria Intergovernamental",
            "Portaria Interministerial",
            "Portaria Normativa",
            "Processo Disciplinar",
            "Proposta Adicional",
            "Proposta Orçamentária",
            "Protocolo",
            "Protocolo de Intenções",
            "Provimento",
            "Quadro de Pessoal",
            "Quadro Estatístico",
            "Questão Administrativa",
            "Ratificação",
            "Reclamação",
            "Recomendação",
            "Recurso Criminal",
            "Recurso Disciplinar",
            "Recurso Extraordinário",
            "Recurso Ordinário",
            "Regimento Interno",
            "RELAÇÃO",
            "Relatório de Correição",
            "Relatórios",
            "Relatórios de Precatórios",
            "Representação Contra Magistrado",
            "Representação no Interesse da Justiça",
            "Representação para Declaração de Indignidade ou de Incompatibilidade",
            "Republicação",
            "Resolução",
            "Resolucão",
            "Resolução do Senado Federal",
            "Resoluções",
            "Resolucões",
            "Restauração de Autos ",
            "Resultado de Análise",
            "Resultado de Avaliação Técnica",
            "Resultado de Cadastramento",
            "Resultado de Concurso Público",
            "Resultado de Eleição",
            "Resultado de Habilitação",
            "Resultado de Índice Técnico",
            "Resultado de Julgamento",
            "Resultado de Julgamento de Licitação",
            "Resultado de Leilão",
            "Resultado de Proposta",
            "Resultado de Proposta Técnica",
            "Resultado de Qualificação",
            "Retificação",
            "Retificações",
            "Retificação (de Edital)",
            "Retificação de Pauta",
            "Retificação e Título de Outorga de Delegação",
            "Revisão Criminal",
            "Sentenças",
            "Sindicância",
            "Solução de Consulta",
            "Súmula",
            "Súmula Administrativa",
            "Termo de Autorização",
            "Termo de Liberação de Operação",
            "Verificação da Invalidez do Magistrado",
            "Vistas"
        };
    }
}
