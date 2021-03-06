using Business.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Data.Database
{
    public class UsuarioAdapter : Adapter
    {
        private readonly AcademyContext _context;
        // Este bypass es para poder usarlo desde el proyecto UI.Consola sin tener que crear el contexto
        public UsuarioAdapter() { }
        public UsuarioAdapter(AcademyContext context)
        {
            _context = context;
        }

        public List<Usuario> GetAll()
        {
            List<Usuario> usuarios = new List<Usuario>();
            try
            {
                usuarios = _context.Usuarios
                    .Include(u => u.Persona)
                    .ToList();
            }
            catch (Exception e)
            {
                Exception ExceptionManejada = new Exception("Error al recuperar listado de usuarios", e);
                throw ExceptionManejada;
            }
            return usuarios;
        }

        public Business.Entities.Usuario GetOne(int ID)
        {
            try
            {
                return _context.Usuarios.Include(u => u.Persona).FirstOrDefault(u => u.ID == ID);
            }
            catch (Exception e)
            {
                Exception ExceptionManejada = new Exception("Error al recuperar datos de usuario", e);
                throw ExceptionManejada;
            }
            return null;
        }

        protected void Update(Usuario usuario)
        {
            Usuario userAnterior = GetOne(usuario.ID);
            if (usuario.Clave != null && usuario.Clave != userAnterior.Clave)
            {
                usuario.Clave = new Hasher().GenerateHash(usuario.Clave, userAnterior.Salt);
            }
            else
            {
                usuario.Clave = userAnterior.Clave;
            }
            try
            {
                this.OpenConnection();
                SqlCommand sqlSave = new SqlCommand(
                    "UPDATE usuarios SET nombre_usuario = @NombreUsuario, clave = @Clave, " +
                    "habilitado = @Habilitado, id_persona = @IDPersona " +
                    "WHERE ID = @ID"
                    , sqlConn);
                sqlSave.Parameters.Add("@ID", SqlDbType.Int).Value = usuario.ID;
                sqlSave.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 50).Value = usuario.NombreUsuario;
                sqlSave.Parameters.Add("@Clave", SqlDbType.VarChar, 50).Value = usuario.Clave;
                sqlSave.Parameters.Add("@Habilitado", SqlDbType.Bit).Value = usuario.Habilitado;
                sqlSave.Parameters.Add("@IDPersona", SqlDbType.Int).Value = usuario.IDPersona;
                sqlSave.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Exception ExceptionManejada = new Exception("Error al modificar datos de usuario", e);
                throw ExceptionManejada;
            }
            finally
            {
                this.CloseConnection();
            }
        }
        protected void Insert(Usuario usuario)
        {
            usuario.Salt = new Hasher().GenerateSalt();
            usuario.Clave = new Hasher().GenerateHash(usuario.Clave, usuario.Salt);
            try
            {
                this.OpenConnection();
                SqlCommand sqlSave = new SqlCommand(
                    "INSERT INTO usuarios(nombre_usuario, clave, habilitado, id_persona) " +
                    "VALUES (@NombreUsuario, @Clave, @Habilitado, @IDPersona) " +
                    "SELECT @@identity"
                    , sqlConn);
                sqlSave.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 50).Value = usuario.NombreUsuario;
                sqlSave.Parameters.Add("@Clave", SqlDbType.VarChar, 50).Value = usuario.Clave;
                sqlSave.Parameters.Add("@Habilitado", SqlDbType.Bit).Value = usuario.Habilitado;
                sqlSave.Parameters.Add("@IDPersona", SqlDbType.Int).Value = usuario.IDPersona;
                usuario.ID = Decimal.ToInt32((decimal)sqlSave.ExecuteScalar());
            }
            catch (Exception e)
            {
                Exception ExceptionManejada = new Exception("Error al crear usuario", e);
                throw ExceptionManejada;
            }
            finally
            {
                this.CloseConnection();
            }
        }
        public void Delete(int ID)
        {
            Usuario usr = new Usuario();
            try
            {
                this.OpenConnection();
                SqlCommand sqlDelete = new SqlCommand("delete usuarios where ID = @id", sqlConn);
                sqlDelete.Parameters.Add("@id", SqlDbType.Int).Value = ID;
                sqlDelete.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Exception ExceptionManejada = new Exception("Error al eliminar usuario", e);
                throw ExceptionManejada;
            }
            finally
            {
                this.CloseConnection();
            }
        }

        public void Save(Usuario usuario)
        {
            if (usuario.State == BusinessEntity.States.New)
            {
                this.Insert(usuario);
            }
            else if (usuario.State == BusinessEntity.States.Deleted)
            {
                this.Delete(usuario.ID);
            }
            else if (usuario.State == BusinessEntity.States.Modified)
            {
                this.Update(usuario);
            }
            usuario.State = BusinessEntity.States.Unmodified;
        }
        public Usuario Login(string usuario, string contrasenia)
        {
            try
            {
                Usuario usr = _context.Usuarios.Include(u => u.Persona).FirstOrDefault(u => u.NombreUsuario == usuario);
                if (usr == null) return null;
                contrasenia = new Hasher().GenerateHash(contrasenia, usr.Salt);
                if (usr.Clave != contrasenia) return null;
                return usr;
            }
            catch (Exception Ex)
            {
                Exception ExcepcionManejada = new Exception("Error al recuperar usuario", Ex);
                throw ExcepcionManejada;
            }
        }
    }
}
