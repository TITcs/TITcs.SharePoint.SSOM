using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TITcs.SharePoint.SSOM;

namespace TITcs.SharePoint.SSOM.Test
{
    [SharePointList("Melhores Práticas Notificação")]
    public class MelhoresPraticasNotificacaoRepository : SharePointRepository<MelhoresPraticasNotificacaoItem>, IMelhoresPraticasNotificacaoRepository
    {
        #region constructors

        public MelhoresPraticasNotificacaoRepository(SPWeb web) : base(web)
        {
        }
        public MelhoresPraticasNotificacaoRepository() : base()
        {
        }

        #endregion

        #region events and methods

        public MelhoresPraticasNotificacaoItem GetByPostAndType(int postId, string type)
        {
            var caml = string.Format(@"<Where>
                            <And>
                                <Eq>
                                    <FieldRef Name='IdPost' />
                                    <Value Type='Integer'>{0}</Value>
                                </Eq>
                                <Eq>
                                    <FieldRef Name='Tipo' />
                                    <Value Type='Choice'>{1}</Value>
                                </Eq>
                            </And>
                        </Where>", postId, type);
            return GetAll(caml).OfType<MelhoresPraticasNotificacaoItem>().ToList<MelhoresPraticasNotificacaoItem>().FirstOrDefault();
        }
        public MelhoresPraticasNotificacaoItem Add(MelhoresPraticasNotificacaoItem item)
        {
            // TODO: Classe Fields deve aceitar Expression<TEntity>[] em seu construtor para facilitar inicialização (PROPOSTA)
            var fields = new Fields<MelhoresPraticasNotificacaoItem>();
            fields.Add(i => i.Title, item.Title);
            fields.Add(i => i.IdPost, item.IdPost);
            fields.Add(i => i.Criador, item.Criador);
            fields.Add(i => i.Tipo, item.Tipo);
            //fields.Add(i => i.Contador, item.Contador);
            fields.Add(i => i.Visualizado, item.Visualizado);
            return GetById(Insert(fields));
        }
        public void Update(MelhoresPraticasNotificacaoItem item)
        {
            var fields = new Fields<MelhoresPraticasNotificacaoItem>();
            fields.Add(i => i.Id, item.Id);
            if (!string.IsNullOrEmpty(item.Title))
                fields.Add(i => i.Title, item.Title);            
            fields.Add(i => i.IdPost, item.IdPost);
            fields.Add(i => i.Criador, item.Criador);
            fields.Add(i => i.Tipo, item.Tipo);
            fields.Add(i => i.Contador, item.Contador);            
            fields.Add(i => i.Visualizado, item.Visualizado);
            fields.Add(i => i.HistoricoCurtidas, item.HistoricoCurtidas);
            Update(fields);
        }
        public IList<MelhoresPraticasNotificacaoItem> GetByCriador(string creator) {
            var caml = string.Format(@"<Where>
                            <And>
                                <Eq>
                                    <FieldRef Name='Criador' />
                                    <Value Type='Text'>{0}</Value>
                                </Eq>
                                <Geq>
                                    <FieldRef Name='Contador' />
                                    <Value Type='Integer'>1</Value>
                                </Geq>
                            </And>
                        </Where>", creator);
            return GetAll(caml).OfType<MelhoresPraticasNotificacaoItem>().ToList<MelhoresPraticasNotificacaoItem>();
        }
        public void Remove(int postId, string type) {
            // get by post and type to remove
            var _notification = GetByPostAndType(postId, type);
            if (_notification != null)
            {
                Delete(_notification.Id);
            }
        }

        #endregion
    }

    public interface IMelhoresPraticasNotificacaoRepository
    {
        IList<MelhoresPraticasNotificacaoItem> GetByCriador(string creator);
        MelhoresPraticasNotificacaoItem GetByPostAndType(int postId, string type);
        MelhoresPraticasNotificacaoItem Add(MelhoresPraticasNotificacaoItem item);
        void Update(MelhoresPraticasNotificacaoItem item);
        void Remove(int postId, string type);
    }
}
